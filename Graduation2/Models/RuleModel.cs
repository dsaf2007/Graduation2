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

namespace Rule.Models
{
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
        // 엑셀 입력 데이터
        public string singleInput { get; set; }
        // rule passed
        public bool isPassed { get; set; }

        public List<Subject> requiredSubjects { get; set; } 
      
        public CheckStrategy checkStrategy { get; set; }

        public string errMessage { get; set; }

        public Rule(CheckStrategy checkStrategy,
                    string type, string keyword, string sequenceNumber, string question, 
                    string singleInput = "", List<Subject> requiredSubjects = null
                    ) 
        {
          isPassed = false;
          this.checkStrategy = checkStrategy;
          this.type = type;
          this.keyword = keyword;
          this.sequenceNumber = sequenceNumber;
          this.question = question;
          this.singleInput = singleInput;
          this.requiredSubjects = requiredSubjects;
        }
        
        public void CheckRule() {
          isPassed = checkStrategy.CheckRule();
          errMessage = checkStrategy.errMessage;
        }
    }
    public abstract class CheckStrategy
    {
      public Rule rule {get; set;}
      public Dictionary<string, List<UserSubject>> userSubjectPair { get; set; }
      public Dictionary<string, int> userCreditPair { get; set; }
      public string errMessage { get; set; }

      public abstract bool CheckRule();
    }
    public class NumberValueChecker : CheckStrategy
    {
      public double userValue {get; set;}
      public double requiredValue {get; set;}

      public NumberValueChecker(Rule rule, 
                                Dictionary<string, List<UserSubject>> _userSubjectPair,
                                Dictionary<string, int> _userCreditPair,
                                double userValue, double requiredValue)
      {
        this.rule = rule;
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
          errMessage = String.Format("[{0}] 수강학점:{1}, 졸업요건:{2}", keyword, userValue, requiredValue);
          return false;
        }
        return true;
      }
    }
    public class OXValueChecker : CheckStrategy
    {
      public string userOX;
      public OXValueChecker(Rule _rule, 
                                Dictionary<string, List<UserSubject>> _userSubjectPair,
                                Dictionary<string, int> _userCreditPair,
                                string _userOX)
      {
        rule = _rule;
        userSubjectPair = _userSubjectPair;
        userCreditPair = _userCreditPair;
        // OX 판별하는 클래스에 추가되는 데이터
        userOX = _userOX;
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
      // public bool takeAll { get; set; }

      public MultiValueChecker(Rule _rule, 
                                Dictionary<string, List<UserSubject>> _userSubjectPair,
                                Dictionary<string, int> _userCreditPair
                              )
      {
        this.rule = rule;
        this.userSubjectPair = userSubjectPair;
        this.userCreditPair = userCreditPair;
        // takeAll = rule.question.Contains("필수")? true : false;
      }
      public override bool CheckRule()
      {
        matches = 0;
        string keyword = rule.keyword;
        List<Subject> requiredSubjects = rule.requiredSubjects;
        List<UserSubject> userSubjects = userSubjectPair[keyword];

        foreach(Subject reqSubject in requiredSubjects) {
          foreach(UserSubject userSubject in userSubjects) {
            if (reqSubject.subjectCode.Equals(userSubject)) {
              matches += 1;
            }
          }
        }
        if (matches < requiredCount)
        {
          errMessage = String.Format("[{0}] 수강과목수:{1}, 졸업요건:{2}과목 수강", keyword, matches, requiredCount);
          return false;
        }
        return true;
      }
    }
}