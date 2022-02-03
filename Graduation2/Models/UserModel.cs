using System.Runtime.CompilerServices;
using System.Net.Cache;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ExcelDataReader;
using MySql.Data.MySqlClient;
//using Rule.Models;

namespace Graduation2.Models
{
  public class TempRule
  {
    public string keyword;
    public string question_type;
    public string value;
  }
  public class UserInfo // 취득교과목
  {

    //public List<Rule.Models.Rule> rule = new List<Rule.Models.Rule>();
    public List<TempRule> rule = new List<TempRule>();
    public List<ListPair> subjectNameList = new List<ListPair>();
    public List<NumPair> subjectCreditList = new List<NumPair>();
    

    public void getRule()
    {
        using (MySqlConnection connection  = new MySqlConnection("Server=101.101.216.163/;Port=5555;Database=testDB;Uid=CSDC;Pwd=1q2w3e4r"))
        {
        string selectQuery = "SELECT * FROM rule";
        connection.Open();
        MySqlCommand command = new MySqlCommand(selectQuery, connection);

        using (var reader = command.ExecuteReader())
        {
            while(reader.Read())
            {
            // TempRule temp = new TempRule();
            // temp.keyword=reader["keyword"].ToString();
            // temp.question_type=reader["question_type"].ToString();
            // temp.value = reader["value"].ToString();
            rule.Add(new TempRule{
                reader["keyword"].ToString(),
                reader["question_type"].ToString(),
                reader["value"].ToString()
                });
            }
        }
        }
    }

    public void initPairList()
    {
        using (MySqlConnection connection  = new MySqlConnection("Server=101.101.216.163/;Port=5555;Database=testDB;Uid=CSDC;Pwd=1q2w3e4r"))
        {
        string selectQuery = "SELECT DISTINCT keyword FROM rule";
        connection.Open();
        MySqlCommand command = new MySqlCommand(selectQuery, connection);
        List<string> tempList = new List<string>();

        using (var reader = command.ExecuteReader())
        {
            while(reader.Read())
            {
                subjectNameList.Add(new ListPair{
                    reader["keyword"].ToString(),
                    tempList
                });
                subjectCreditList.add(new NumPair{
                    reader["keyword"].ToString()
                })
            }
        }
    }

    public void getUserSubject(string filename_)
    {
        // using (var subjectStream = System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        // {
        //     using (var subjectReader = ExcelReaderFactory.CreateReader(SubjectStream))
        //     {
        //         subjectReader.Read();
        //         using (MysqlConnection connection = new MysqlConnection("Server=101.101.216.163/;Port=5555;Database=testDB;Uid=CSDC;Pwd=1q2w3e4r"))
        //         {
        //             while(subjectReader.Read())
        //             {
        //             // string insertQuery = string.Format("INSERT INTO UserSubject(year,semester,completionDiv,completionDivField,SubjectCode,"
        //             //                     +"SubjectName,credit,engineeringFactor,engineeringFactorDetail,english,retake)"
        //             //                     +"VALUES ( '{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}')"
        //             //                     ,subjectReader
        //             }
        //         }

        //     }
        // }
        public List<Subject> userSubject = new List<Subject>();

        userSubject = ReadUserSubject(filename_)



         foreach (UserSubject userSubject in userSubject_)
            {
                int subjectCredit = Convert.ToInt32(userSubject.credit);
                this.totalCredit += subjectCredit;

                if (userSubject.engineeringFactorDetail == "기초교양(교필)")
                {
                    this.publicLibCredit += subjectCredit;
                    this.publicClasses.Add(userSubject);
                }
                if (userSubject.engineeringFactorDetail == "기본소양")
                {
                    this.basicLibCredit += subjectCredit;
                    this.basicClasses.Add(userSubject);

                }
                if (userSubject.engineeringFactor == "MSC/BSM")
                {
                    this.mscCredit += subjectCredit;
                    switch (userSubject.engineeringFactorDetail)
                    {
                        case "수학":
                            this.mscMathCredit += subjectCredit;
                            break;
                        case "기초과학":
                            if (userSubject.className.Contains("실험"))
                                this.mscScienceExperimentCredit += subjectCredit;
                            this.mscScienceCredit += subjectCredit;
                            break;
                        case "전산학":
                            this.mscComputerCredit += subjectCredit;
                            break;
                        default:
                            break;
                    }
                    this.mscClasses.Add(userSubject);
                }
                if (userSubject.engineeringFactor == "전공" || userSubject.completionDiv == "전공")
                {
                    this.majorCredit += subjectCredit;
                    if (userSubject.completionDiv == "전필")
                    {
                        this.majorEssentialList.Add(userSubject);
                        this.majorEssentialCredit += subjectCredit;
                    }
                    if (userSubject.completionDivField == "전문")
                    {
                        this.majorSpecialCredit += subjectCredit;
                    }
                    if (userSubject.engineeringFactorDetail == "전공설계")
                    {
                        this.majorDesignCredit += subjectCredit;
                        this.majorDesignList.Add(userSubject);
                        this.majorEssentialList.Add(userSubject);
                    }
                    if (userSubject.english == "영어")
                    {
                        this.englishMajorCredit += subjectCredit;
                        this.englishMajorList.Add(userSubject);

                    }
                    this.majorClasses.Add(userSubject);
                }
                if (userSubject.english == "영어")
                {
                    this.englishCredit += subjectCredit;
                    this.englishList.Add(userSubject);
                }
            }
    }
    
    public List<UserSubject> ReadUserSubject(string filename_)
    {
        List<UserSubject> temp = new List<UserSubject>();

        // 전체성적조회파일
        using (var gradeStream = System.IO.File.Open(filename_, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            using (var gradeReader = ExcelReaderFactory.CreateReader(gradeStream))
            {
                gradeReader.Read();
                string tempYear = "";
                string tempSemester = "";
                while (gradeReader.Read())
                {
                    string[] valueArray = new string[19];
                    for (int i = 0; i < 19; i++)
                    {
                        if (gradeReader.GetValue(i) == null)
                            valueArray[i] = "";
                        else
                            valueArray[i] = Regex.Replace(gradeReader.GetValue(i).ToString(), @"\s", "");
                    }
                    if (valueArray[2] != "")
                    {
                        tempYear = valueArray[2];
                    }
                    if (valueArray[3] != "")
                    {
                        tempSemester = valueArray[3];
                    }
                    temp.Add(new UserSubject
                    {
                        year = tempYear, // 연도
                        semester = tempSemester, // 학기
                        completionDiv = valueArray[4], // 이수구분 : 전공, 전필, 학기, 공교 등
                        completionDivField = valueArray[5], // 이수구분영역 : 기초, 전문, 자연과학 등
                        classCode = valueArray[6], // 학수번호
                        className = valueArray[8], // 과목명
                        credit = valueArray[10], // 학점
                        engineeringFactor = valueArray[16], // 공학요소 : 전공, MSC, 전문교양
                        engineeringFactorDetail = valueArray[17], // 공학세부요소 : 전공설계, 수학, 과학 등
                        english = valueArray[18], // 원어강의 종류
                        retake = valueArray[13] //재수강 여부
                    }); 
                }
            }
        }
        return temp;
    }

  
  }
}