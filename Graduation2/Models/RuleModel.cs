using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Cache;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        Rule newRule = new Rule(division, sequenceNumber, question, replyType, singleInput, null);
        newRule.SetKeyword();
        return newRule;
      }
    }
}