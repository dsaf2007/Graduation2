using System.Runtime.CompilerServices;
using System.Net.Cache;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
// using ExcelDataReader;
// using MySql.Data.MySqlClient;

namespace Graduation2.Models
{
  public class UserInfo
  {// 취득교과목
    const List<String> keywords = 
    {
    "공통교양", "기본소양", "수학", // MSC랑 수학과학전산학은 어떻게?
    "과학", // 실험 포함? 따로?
    "전산학", "전공", // 전공 안에 필수, 설계 등을 type 형태로 구분?
    "전공필수", "전공설계",
    "기초설계", // 얘네는 성적표에 '전공설계'로 나옴
    "요소설계", "종합설계",
    };
    // <keyword - 과목리스트>
    public Dictionary<string, List<UserSubject>> subjectPair;
    public int calcKeywordCredit()
    {
      return 0;
    }
    /*
      - publicLibCredit # 수강한 총 공통교양 학점
      - basicLibCredit # 수강한 총 기본소양 학점
      - majorCredit # 수강한 총 전공학점
      - majorDesignCredit # 수강한 총 전공 설계 학점
      - majorEssentialCredit # 수강한 총 전공 필수 학점
      - majorSpecialCredit # 수강한 총 전공 전문 학점
      - mscCredit # 수강한 총 MSC 학점
      - mscMathCredit # MSC 수학 / 과학 (실험) / 컴퓨터
      - mscScienceCredit
      - mscScienceExperimentCredit
      - mscComputerCredit
      - englishCredit # 영어 강의
      - englishMajorCredit # 영어 전공 강의
      - totalCredit # 전체 수강한 학점
      - gradeAverage # 학점 평균
    */
  }
}