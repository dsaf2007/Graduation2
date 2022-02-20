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

namespace Graduation2.Models
{
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
          string errorMessage = String.Format("[{0}] 수강학점: {1}, 졸업요건: {2}학점 ({1}/{2}) 필요학점: {3}학점",
                                      keyword, userValue, requiredValue, requiredValue-userValue);
          rule.SetResultMessage(errorMessage);
          return false;
        }
        else
        {
          string successMessage = String.Format("[{0}] 수강학점: {1}, 졸업요건: {2}학점",
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
          string errMessage = String.Format("[{0}] 수강과목수: {1}, 졸업요건: {2}과목 ({1}/{2}), 필요 과목수: {3}  ",
                                      keyword, matches, requiredCount, requiredCount-matches);

          string neededSubjectsString = String.Join<string>(", ", neededSubjects);
          string notice = rule.shouldTakeAll? "[필요과목]" : "[수강가능과목]";
          errMessage += String.Format("{0}: {1}", notice, neededSubjectsString);

          rule.SetResultMessage(errMessage);    
          return false;
        }
        else
        {
          string takenSubjectsString = String.Join<string>(", ", takenSubjects);
          string successMessage = String.Format("[{0}] 수강과목수: {1}, 졸업요건: {2}과목  ",
                                      keyword, matches, requiredCount);

          if (takenSubjects.Count > 0)
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
}