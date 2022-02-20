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
        public string division { get; set; }
        // keyword (subject.keyword랑 매치되어야함)
        public string keyword { get; set; }
        // 일련번호
        public string sequenceNumber { get; set; }
        // 질문
        public string question { get; set; }
        // 엑셀 입력 데이터 -> 웹버전으로 바뀌어야함
        public string singleInput { get; set; }
        public List<Subject> requiredSubjects { get; set; } 

        // 단수, 목록, OX
        public string replyType { get; set; }
        // rule 패스 여부
        public bool isPassed { get; set; }
        // 전체 필수?
        public bool shouldTakeAll {get; set;}
      
        public CheckStrategy checkStrategy { get; set; }

        public string resultMessage { get; set; }
        public string errMessage { get; set; }

        public Rule(string division, 
                    string sequenceNumber, string question, string replyType,
                    string singleInput="", List<Subject> requiredSubjects=null
                    ) 
        {
          isPassed = false;
          this.division = division;
          this.sequenceNumber = sequenceNumber;
          this.question = question;
          this.replyType = replyType;
          this.singleInput = singleInput;
          this.requiredSubjects = requiredSubjects;
          this.shouldTakeAll = shouldTakeAll;
          this.shouldTakeAll = this.question.Contains("필수")? true : false;
        }

        public void SetCheckStrategy(CheckStrategy checkStrategy)
        {
          this.checkStrategy = checkStrategy;
          this.checkStrategy.rule = this;
        }
    
        public void SetKeyword() {
          // List<string> keywords = new List<string>();
          string keyword = "";
          string firstWord = question.Split()[0];
          if(UserInfo.ruleKeywords.Contains(firstWord))
          {
            keyword = firstWord;
            if(question.Contains("과학") && question.Contains("실험"))
              keyword += "실험";
            if(question.Contains("전공") && question.Contains("필수"))
              keyword += "필수";
          }
          else // debug
            Console.WriteLine($"[Error] 룰에서 키워드 추출에 실패 Rule#{sequenceNumber} at Rule_SetKeyword()"); 
          this.keyword = keyword;
        }
        public void CheckRule() 
        {
          isPassed = checkStrategy.CheckRule();
        }
        public void SetResultMessage(string resultMessage) {
          this.resultMessage = resultMessage;
        }
    }
    public abstract class CheckStrategy
    {
      public Rule rule {get; set;}
      public UserInfo userInfo {get; set;}
      public Dictionary<string, List<UserSubject>> userSubjectPair { get; set; }
      public Dictionary<string, int> userCreditPair { get; set; }
      // public string errMessage { get; set; }

      public abstract bool CheckRule();
    }

    public class NumberValueChecker : CheckStrategy
    {
      public double userValue {get; set;}
      public double requiredValue {get; set;}

      public NumberValueChecker(UserInfo userInfo)
      {
        this.userSubjectPair = userInfo.keywordSubjectPair;
        this.userCreditPair = userInfo.keywordCreditPair;
        this.userInfo = userInfo;
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
      public OXValueChecker(UserInfo userInfo)
      {
        this.userSubjectPair = userInfo.keywordSubjectPair;
        this.userCreditPair = userInfo.keywordCreditPair;
        // OX 룰 관련해서는 userInfo 구조 어떻게 쓸지?
        // dictionary[keyword] 형태로 다시 만들지
        // 기존처럼 영어PASS:O 이런 데이터 하드코딩해서 쓸지..
        // OX 룰은 '구분'이 교양,전공에는 없고 졸업요건 룰부터 있음
        this.userInfo = userInfo; 

      }
      public override bool CheckRule() 
      {
        // todo; temp value
        userOX = "X";
        string condition = rule.singleInput; // O or X
        if ("X".Equals(condition.ToUpper()))
          return true;
        
        return condition.ToUpper().Equals(userOX.ToUpper());
      }

      public void SetUserOX()
      {
        // todo
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
      public MultiValueChecker(UserInfo userInfo)
      {
        this.userSubjectPair = userInfo.keywordSubjectPair;
        this.userCreditPair = userInfo.keywordCreditPair;

        requiredCount = 0;
        requiredCredit = 0;
      }
      public override bool CheckRule()
      {
        matches = 0;
        int takenCredit = 0;
        string keyword = rule.keyword;
        // if(String.IsNullOrEmpty(keyword)) {
        //   keyword = "공통교양";
        // }
        List<UserSubject> userSubjects = new List<UserSubject>();
        userSubjects = userSubjectPair[keyword];
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
        List<string> neededSubjects = requiredSubjects.Select(subject => subject.subjectName).ToList<string>();

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
          string errMessage = String.Format("[{0}] 수강과목수: {1}, 졸업요건: {2}과목, 필요 과목수: {3} ",
                                      keyword, matches, requiredCount, requiredCount-matches);

          string neededSubjectsString = String.Join<string>(",", neededSubjects);
          string notice = rule.shouldTakeAll? "[필요과목]" : "[수강가능과목]";
          errMessage += String.Format("{0}: {1}", notice, neededSubjectsString);

          rule.SetResultMessage(errMessage);    
          return false;
        }
        else
        {
          string takenSubjectsString = String.Join<string>(",", takenSubjects);
          string successMessage = String.Format("[{0}] 수강과목수: {1}, 졸업요건: {2}과목 ",
                                      keyword, matches, requiredCount);
          successMessage += String.Format("[수강한 과목] {0}", takenSubjectsString);
          rule.SetResultMessage(successMessage);
          return true;
        }
      }
    }


    public class NoCheckStrategy : CheckStrategy
    {
      public override bool CheckRule()
      {
        return false;
      }
    }


    public class RuleBuilder
    {
        public string division = "";
        // 일련번호
        public string sequenceNumber = "";
        // 질문
        public string question = "";
        // 엑셀 입력 데이터
        public string singleInput = "";
        // 비고
        public string replyType = "";

      
      public RuleBuilder () {}

      public RuleBuilder SetDivision(string division)
      {
        this.division = division;
        return this;
      }
      public RuleBuilder SetSequenceNumber(string sequenceNumber)
      {
        this.sequenceNumber = sequenceNumber;
        return this;
      }
      public RuleBuilder SetQuestion(string question)
      {
        question = Regex.Replace(question, @"[을를]\s\S*하세요.", " 만족 여부 확인");
        this.question = question;
        return this;
      }
      public RuleBuilder SetSingleInput(string singleInput)
      {
        this.singleInput = singleInput;
        return this;
      }
      public RuleBuilder SetReplyType(string replyType)
      {
        this.replyType = replyType;
        return this;
      }

      public Rule Build()
      {
        int ruleFlag = -1;
        
        // 기본정보 룰인지 체크. 비고란 비어있지 않을때
        if(division != "기초정보")
        {
          if(replyType == "단수" || replyType == "OX") 
          {
            ruleFlag = 0;
            if(replyType == "OX")
            {
              singleInput = singleInput.ToUpper();
              ruleFlag = 1;
            }
          }
          if(replyType == "목록") 
          {
            ruleFlag = (question.Contains("필수") 
              || question.Contains("설계"))
              ? 3 : 2;
          }
        }
        Rule newRule = new Rule(division, sequenceNumber, question, replyType, singleInput, null);
        newRule.SetKeyword();
        return newRule;
      }
    }
}