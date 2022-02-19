using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Net.Cache;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Graduation2.Models;
// using ExcelDataReader;
// using MySql.Data.MySqlClient;

namespace Graduation2.Models
{
    // 드롭다운 값에 따라 룰 멤버변수 변경해야함
    // 엑셀에 맞춰진 룰 형식
    public class Rule
    {
        // 구분 (교양, 전공, 졸업요건, 예외)
        public string type { get; set; }
        // keyword (subject.keyword랑 매치되어야함)
        public string keyword { get; set; }
        // 일련번호
        public string sequenceNumber { get; set; }
        // 질문
        public string question { get; set; }
        // 엑셀 입력 데이터 -> 웹버전으로 바뀌어야함
        public string singleInput { get; set; }
        public List<Subject> requiredSubjects { get; set; } 
        // rule 패스 여부
        public bool isPassed { get; set; }
        // 전체 필수?
        public bool shouldTakeAll {get; set;}
      
        public CheckStrategy checkStrategy { get; set; }

        public string resultMessage { get; set; }
        public string errMessage { get; set; }

        public Rule(CheckStrategy checkStrategy,
                    string type, string keyword, string sequenceNumber, string question, 
                    string singleInput="", List<Subject> requiredSubjects=null
                    ) 
        {
          isPassed = false;
          this.checkStrategy = checkStrategy;
          this.checkStrategy.rule = this;
          this.type = type;
          this.keyword = keyword;
          this.sequenceNumber = sequenceNumber;
          this.question = question;
          this.singleInput = singleInput;
          this.requiredSubjects = requiredSubjects;
          this.shouldTakeAll = shouldTakeAll;
          this.shouldTakeAll = this.question.Contains("필수")? true : false;
        }
        
        public void CheckRule() {
          isPassed = checkStrategy.CheckRule();
          // errMessage = isPassed ? "" : checkStrategy.errMessage;
        }
        public void SetErrorMessage(string errorMessage) {
          this.errMessage = errMessage;
        }
        public void SetResultMessage(string resultMessage) {
          this.resultMessage = resultMessage;
        }
    }
    public abstract class CheckStrategy
    {
      public Rule rule {get; set;}
      public Dictionary<string, List<UserSubject>> userSubjectPair { get; set; }
      public Dictionary<string, int> userCreditPair { get; set; }
      // public string errMessage { get; set; }

      public abstract bool CheckRule();
    }

    public class NumberValueChecker : CheckStrategy
    {
      public double userValue {get; set;}
      public double requiredValue {get; set;}

      public NumberValueChecker(Dictionary<string, List<UserSubject>> userSubjectPair,
                                Dictionary<string, int> userCreditPair,
                                double userValue, double requiredValue)
      {
        this.userSubjectPair = userSubjectPair;
        this.userCreditPair = userCreditPair;
        this.userValue = userValue;
        this.requiredValue = requiredValue;
      }
      public override bool CheckRule() 
      {
        string keyword = rule.keyword;

        userValue = Convert.ToDouble(userCreditPair[keyword]);
        requiredValue = Convert.ToDouble(rule.singleInput);

        if (userValue < requiredValue)
        {
          string errorMessage = String.Format("[{0}] 현재 수강학점: {1}, 졸업요건: {2}, 필요학점: {3}",
                                      keyword, userValue, requiredValue, requiredValue-userValue);
          rule.SetResultMessage(errorMessage);
          return false;
        }
        else
        {
          string successMessage = String.Format("[{0}] 현재 수강학점: {1}, 졸업요건: {2}",
                                      keyword, userValue, requiredValue);
          rule.SetResultMessage(successMessage);
          return true;
        }
      }
    }
    public class OXValueChecker : CheckStrategy
    {
      public string userOX;
      public OXValueChecker(Dictionary<string, List<UserSubject>> userSubjectPair,
                            Dictionary<string, int> userCreditPair,
                            string userOX)
      {
        this.userSubjectPair = userSubjectPair;
        this.userCreditPair = userCreditPair;
        // OX 판별하는 클래스에 추가되는 데이터
        this.userOX = userOX;
      }
      public override bool CheckRule() 
      {
        string condition = rule.singleInput; // O or X
        if ("X".Equals(condition.ToUpper()))
          return true;
        
        return condition.ToUpper().Equals(userOX.ToUpper());
      }
    }
    public class MultiValueChecker : CheckStrategy
    {
      public int matches { get; set; }
      public int Matches
      { get
        {
          return matches;
        }
      }
      public int requiredCount { get; set; }
      public int RequiredCount
      {
        get
        {
          return requiredCount;
        }
      }
      public int requiredCredit { get; set; }
      public int RequiredCredit
      {
        get
        {
          return RequiredCredit;
        }
      }
      public MultiValueChecker(Dictionary<string, List<UserSubject>> userSubjectPair,
                               Dictionary<string, int> userCreditPair
                              )
      {
        this.userSubjectPair = userSubjectPair;
        this.userCreditPair = userCreditPair;

        requiredCount = 0;
        requiredCredit = 0;
      }
      public override bool CheckRule()
      {
        matches = 0;
        int takenCredit = 0;
        string keyword = rule.keyword;
        List<UserSubject> userSubjects = userSubjectPair[keyword];
        List<Subject> requiredSubjects = rule.requiredSubjects;

        if (rule.shouldTakeAll)
        {
          requiredCount = requiredSubjects.Count;
          foreach(Subject subject in requiredSubjects)
          {
            requiredCredit += subject.credit;
          }
        }
        else
        {
          requiredCredit = userCreditPair[keyword];
        }

        // 수강한 과목
        List<string> takenSubjects = new List<string>();
        // 수강이 필요한 과목
        List<string> neededSubjects = requiredSubjects.Select(subject => subject.subjectCode).ToList<string>();

        foreach(Subject reqSubject in requiredSubjects) {
          foreach(UserSubject userSubject in userSubjects) {
            if (reqSubject.IsSameSubject(userSubject)) {
              matches += 1;
              takenCredit += userSubject.credit;
              neededSubjects.Remove(userSubject.subjectName);
              takenSubjects.Add(userSubject.subjectName);
              break;
            }
          }
        }
        if (matches < requiredCount || takenCredit < requiredCredit)
        {
          string errMessage = String.Format("[{0}] 현재 수강과목수: {1}, 졸업요건: 총 {2}과목 수강, 필요 수강과목수: {3}",
                                      keyword, matches, requiredCount, requiredCount-matches);

          string neededSubjectsString = String.Join<string>(",", neededSubjects);
          string notice = rule.shouldTakeAll? "필요과목" : "수강가능과목";
          errMessage += String.Format("\n{0}: {1}", notice, neededSubjectsString);

          rule.SetResultMessage(errMessage);    
          return false;
        }
        else
        {
          string takenSubjectsString = String.Join<string>(",", takenSubjects);
          string successMessage = String.Format("[{0}] 현재 수강과목수: {1}, 졸업요건: 총 {2}과목 수강",
                                      keyword, matches, requiredCount);
          successMessage += String.Format("\n 수강한 과목: {0}", takenSubjectsString);
          rule.SetResultMessage(successMessage);
          return true;
        }
      }
    }
}