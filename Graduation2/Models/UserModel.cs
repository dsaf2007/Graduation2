using System.Dynamic;
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
using Graduation2.Models;

namespace Graduation2.Models
{
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
    public class TempRule
    {
        public string keyword { get; set; }
        public string question_type { get; set; }
        public string value { get; set; }

        public bool check { get; set; }
    }
    public class UserInfo // 취득교과목
    {
        //public List<Rule.Models.Rule> rule = new List<Rule.Models.Rule>();
        public List<TempRule> rule = new List<TempRule>();

        public List<string> exceptionList = new List<string>();

        public Dictionary<string, List<UserSubject>> keywordSubjectPair;
        public Dictionary<string, int> keywordCreditPair;

        public Dictionary<string, string> errorMessageList;

        //실제로는 입력 받아야함.
        private string username = "TEST";
        private string major = "컴퓨터공학전공";
        private string applicationYear = "2016";
        private string advancedStatus = "N";

        public string GetUsername()
        {
            return this.username;
        }
        public string GetMajor()
        {
            return this.major;
        }
        public string GetApplicationYear()
        {
            return this.applicationYear;
        }

        public int totalCredit;

        private string[] basicList = new string[] { "PRI4041", "PRI4043", "PRI4048", "PRI4040" }; //기본소양 교과목 목록

        public static readonly string[] ruleKeywords = {
            "공통교양", "기본소양", "일반교양",
            "수학", "과학", "전산학", "과학실험", "MSC/BSM",
            "전공", "전공필수", "전공전문", "설계",
            "기초설계", "요소설계", "종합설계", // 얘네는 성적표에 '전공설계'로 나옴
            "영어강의"
        };
        public static readonly DesignSubject[] gichoDesignSubjects = {
          new DesignSubject {
            subjectCode = "CSE2028",
            subjectName = "어드벤처디자인",
            credit = 3,
            designCredit = 3,
            designType = "기초설계"
          },
          new DesignSubject {
            subjectCode = "CSE2016",
            subjectName = "창의적공학설계",
            credit = 3,
            designCredit = 3,
            designType = "기초설계"
          }
        };
        public static readonly DesignSubject[] yosoDesignSubjects = {
          new DesignSubject {
            subjectCode = "CSE4074",
            subjectName = "공개SW프로젝트",
            credit = 3,
            designCredit = 3,
            designType = "요소설계"
          }
        };
        public static readonly DesignSubject[] jonghabDesignSubjects = {
          new DesignSubject {
            subjectCode = "CSE4066",
            subjectName = "컴퓨터공학종합설계1",
            credit = 3,
            designCredit = 3,
            designType = "종합설계"
          },
          new DesignSubject {
            subjectCode = "CSE4067",
            subjectName = "컴퓨터공학종합설계2",
            credit = 3,
            designCredit = 3,
            designType = "종합설계"
          }
        };

        public UserInfo()
        {
            keywordSubjectPair = new Dictionary<string, List<UserSubject>>();
            keywordCreditPair = new Dictionary<string, int>();
            foreach (string keyword in ruleKeywords)
            {
                keywordSubjectPair.Add(keyword, new List<UserSubject>());
                keywordCreditPair.Add(keyword, 0);
            }
            totalCredit = 0;
        }

        public void GetRule()
        {
            using (MySqlConnection connection = new MySqlConnection("Server=101.101.216.163/;Port=5555;Database=testDB;Uid=CSDC;Pwd=1q2w3e4r"))
            {
                string selectQuery = "SELECT * FROM rule";
                connection.Open();
                MySqlCommand command = new MySqlCommand(selectQuery, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rule.Add(new TempRule
                        {
                            keyword = reader["keyword"].ToString(),
                            question_type = reader["question_type"].ToString(),
                            value = reader["value"].ToString(),
                            check = false
                        });
                        errorMessageList.Add(reader["keyword"].ToString(), "");
                    }
                }
            }
        }


        public void GetUserSubject(string studentScoreFile)
        {
            
            List<UserSubject> userSubjects = new List<UserSubject>();
            userSubjects = ReadUserSubject(studentScoreFile);

            foreach (UserSubject userSubject in userSubjects)
            {
                List<string> keywordsOfSubject = userSubject.GetKeywords();
                foreach (string keyword in keywordsOfSubject)
                {
                    keywordSubjectPair[keyword].Add(userSubject);
                    keywordCreditPair[keyword] += userSubject.credit;
                }
                totalCredit += userSubject.credit;
                // debug
                Console.WriteLine($"{userSubject.subjectName}");
            }
            // debug
            PrintUserSubjects();
        }
        // debug
        public void PrintUserSubjects()
        {
            foreach (string key in keywordSubjectPair.Keys)
            {
                Console.WriteLine("<{0}> 총 {1}과목 {2}학점 수강", key, keywordSubjectPair[key].Count, keywordCreditPair[key]);
                foreach (UserSubject subject in keywordSubjectPair[key])
                {
                    Console.WriteLine("[{0}] {1}", subject.year + "-" + subject.semester, subject.subjectName);
                }
                Console.WriteLine();
            }
        }

        // 사용자 성적 파일 READ
        public List<UserSubject> ReadUserSubject(string studentScoreFile)
        {
            List<UserSubject> temp = new List<UserSubject>();

            // 전체성적조회파일
            using (var gradeStream = System.IO.File.Open(studentScoreFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (var gradeReader = ExcelReaderFactory.CreateReader(gradeStream))
                {
                    gradeReader.Read();
                    string tempYear = "";
                    string tempSemester = "";
                    while (gradeReader.Read())
                    {
                        string[] valueArray = new string[20];
                        for (int i = 0; i < 20; i++)
                        {
                            if (gradeReader.GetValue(i) == null)
                                valueArray[i] = "";
                            else // 빈칸 제거
                                valueArray[i] = Regex.Replace(gradeReader.GetValue(i).ToString(), @"\s", "");
                        }
                        if (valueArray[1] != "")
                        {
                            tempYear = valueArray[1];
                        }
                        if (valueArray[2] != "")
                        {
                            tempSemester = valueArray[2];
                        }
                        temp.Add(new UserSubject
                        {
                            year = tempYear, // 연도
                            semester = tempSemester, // 학기
                            completionDiv = valueArray[3], // 이수구분 : 전공, 전필, 학기, 공교 등
                            completionDivField = valueArray[4], // 이수구분영역 : 기초, 전문, 자연과학 등
                            subjectCode = valueArray[5], // 학수번호
                            subjectName = valueArray[7], // 과목명
                            credit = Convert.ToInt32(Convert.ToDouble(valueArray[9])), // 학점
                            engineeringFactor = valueArray[14], // 공학요소 : 전공, MSC, 전문교양
                            engineeringFactorDetail = valueArray[15], // 공학세부요소 : 전공설계, 수학, 과학 등
                            english = valueArray[16], // 원어강의 종류
                            retake = valueArray[12] //재수강 여부
                        });
                    }
                }
            }
            return temp;
        }

        public void CheckException()
        {
            List<UserSubject> temp = keywordSubjectPair["기본소양"];//기본소양 교과목 예외
            foreach (string basicList_ in basicList)
            {
                foreach (UserSubject basicSubject in temp)
                {
                    if (basicList_ == basicSubject.subjectCode)//예외 처리할 과목명 일치시
                    {
                        if (Convert.ToInt32(basicSubject.year) >= 2021)// 수강년도가 2021년 이후
                        {
                            if (basicSubject.retake != "NEW재수강")//재수강이 아닐경우
                            {
                                this.keywordSubjectPair["기본소양"].Remove(new UserSubject() { subjectCode = basicSubject.subjectCode });
                                this.keywordCreditPair["기본소양"] -= Convert.ToInt32(basicSubject.credit);
                                //예외 처리 오류 메시지 입력
                                exceptionList.Add("미 인정 기본 소양 교과목 수강(" + basicSubject.subjectName + ")");
                            }
                        }
                    }
                }

            }
            //이산수학 이산구조 수강
            if (Convert.ToInt32(this.applicationYear) >= 2017) //https://cse.dongguk.edu/?page_id=799&uid=1480&mod=document
            {
                //advancedStatus(심화과정 여부) 입력 받아야함.
                if (this.advancedStatus == "N" && this.applicationYear == "2017")//일반과정
                {
                    bool CSE2026 = false;
                    bool PRI4027 = false;
                    UserSubject tempSubject = new UserSubject();
                    foreach (UserSubject majorEssential in this.keywordSubjectPair["전공전문"])
                    {
                        if (majorEssential.subjectCode == "CSE2026")
                        {
                            CSE2026 = true;
                        }
                    }
                    foreach (UserSubject msc in this.keywordSubjectPair["MSC"])
                    {
                        if (msc.subjectCode == "PRI4027" && msc.year == "2017")
                        {
                            PRI4027 = true;
                            tempSubject = msc;
                        }
                    }
                    if (CSE2026 == false && PRI4027 == true)
                    {
                        keywordSubjectPair["MSC"].Remove(new UserSubject() { subjectCode = "PRI4027" });
                        tempSubject.subjectCode = "CSE2026"; // 학수번호만 변경. 교과목명 유지
                        keywordSubjectPair["전문"].Add(tempSubject);
                        //예외 처리 오류 메시지 입력
                        exceptionList.Add("이산구조 교과목 이산수학으로 대체 인정");
                    }
                }
            }
            UserSubject design1 = new UserSubject();
            UserSubject design2 = new UserSubject();//종합설계 순차 이수.
            bool design1Status = false;
            bool design2Status = false;
            bool fieldPractice = false;

            foreach (UserSubject majorEssential in this.keywordSubjectPair["전공전문"])
            {
                if (majorEssential.subjectCode == "CSE4066")//예외 처리할 과목명 일치시
                {
                    design1 = majorEssential;
                    design1Status = true;
                }
                if (majorEssential.subjectCode == "CSE4067")
                {
                    design2 = majorEssential;
                    design2Status = true;
                }
            }
            foreach (UserSubject majorClassList in this.keywordSubjectPair["전공"])//현장실습
            {
                if ((majorClassList.subjectCode == "ITS4003") || (majorClassList.subjectCode == "ITS4004"))
                {
                    fieldPractice = true;
                }
            }
            if (design1Status == false && fieldPractice == true)
            {
                //예외처리 오류 메시지 입력
                exceptionList.Add("종합설계1의 현장실습 대체 여부를 확인하십시오.");
            }
            else if (design2Status == false && fieldPractice == true)
            {
                //예외처리 오류 메시지 입력
                exceptionList.Add("종합설계2의 현장실습 대체 여부를 확인하십시오.");
            }

            if ((Convert.ToInt32(design1.year) > Convert.ToInt32(design2.year)) && Convert.ToInt32(design2.year) != 0)
            {
                //예외처리 오류 메시지 입력
                exceptionList.Add("종합설계를 순차적으로 이수하지 않았습니다.");
            }
            else if (Convert.ToInt32(design1.year) == Convert.ToInt32(design2.year))
            {
                if (design1.semester == "2학기" && design2.semester == "1학기")
                {
                    //예외처리 오류 메시지 입력
                    exceptionList.Add("종합설계를 순차적으로 이수하지 않았습니다.");
                }
                //같은 학기에 동시에 이수 한 경우 ??
            }

            // 현장실습 종합설계 

            // 동일유사전공교과목 처리
            List<SimillarMajor> simillarList = new List<SimillarMajor>();
            List<DiffMajor> diffMajorList = new List<DiffMajor>();
            using (MySqlConnection connection = new MySqlConnection("Server=101.101.216.163;Port=5555;Database=test;Uid=CSDC;Pwd=1q2w3e4r"))
            {
                string selectQuery = "SELECT * from SIMILLAR";

                connection.Open();
                MySqlCommand command = new MySqlCommand(selectQuery, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PREV_CLASS_START"].ToString() == "null")
                            simillarList.Add(new SimillarMajor
                            {
                                currSubjectName = reader["CURR_CLASS_NAME"].ToString(),
                                currSubjectStartYear = Convert.ToInt32(reader["CURR_CLASS_START"].ToString()),
                                prevSubjectName = reader["PREV_CLASS_NAME"].ToString(),
                                prevSubjectStartYear = 0,//시작년도가 없는 경우 0으로 대체
                                prevSubjectEndYear = Convert.ToInt32(reader["PREV_CLASS_END"].ToString())
                            });
                        else
                            simillarList.Add(new SimillarMajor
                            {
                                currSubjectName = reader["CURR_CLASS_NAME"].ToString(),
                                currSubjectStartYear = Convert.ToInt32(reader["CURR_CLASS_START"].ToString()),
                                prevSubjectName = reader["PREV_CLASS_NAME"].ToString(),
                                prevSubjectStartYear = Convert.ToInt32(reader["PREV_CLASS_START"].ToString()),
                                prevSubjectEndYear = Convert.ToInt32(reader["PREV_CLASS_END"].ToString())
                            });
                    }
                }

                selectQuery = "SELECT * FROM DIFF_MAJOR";
                command = new MySqlCommand(selectQuery, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["START_YEAR"].ToString() == "")
                            diffMajorList.Add(new DiffMajor
                            {
                                startYear = 0,
                                endYear = Convert.ToInt32(reader["END_YEAR"].ToString()),
                                subjectCode = reader["CLASS_CODE"].ToString(),
                                subjectName = reader["CLASS_NAME"].ToString(),
                                otherMajor = reader["OTHER_MAJOR"].ToString(),
                                otherSubjectCode = reader["OTHER_CLASS_CODE"].ToString(),
                                otherSubjectName = reader["OTHER_CLASS_NAME"].ToString()
                            });
                        else if (reader["END_YEAR"].ToString() == "")
                            diffMajorList.Add(new DiffMajor
                            {
                                startYear = Convert.ToInt32(reader["START_YEAR"].ToString()),
                                endYear = 9999,
                                subjectCode = reader["CLASS_CODE"].ToString(),
                                subjectName = reader["CLASS_NAME"].ToString(),
                                otherMajor = reader["OTHER_MAJOR"].ToString(),
                                otherSubjectCode = reader["OTHER_CLASS_CODE"].ToString(),
                                otherSubjectName = reader["OTHER_CLASS_NAME"].ToString()
                            });
                        else
                            diffMajorList.Add(new DiffMajor
                            {
                                startYear = Convert.ToInt32(reader["START_YEAR"].ToString()),
                                endYear = Convert.ToInt32(reader["END_YEAR"].ToString()),
                                subjectCode = reader["CLASS_CODE"].ToString(),
                                subjectName = reader["CLASS_NAME"].ToString(),
                                otherMajor = reader["OTHER_MAJOR"].ToString(),
                                otherSubjectCode = reader["OTHER_CLASS_CODE"].ToString(),
                                otherSubjectName = reader["OTHER_CLASS_NAME"].ToString()
                            });
                    }
                    //}
                    connection.Close();
                }
                temp = this.keywordSubjectPair["전공"];

                foreach (UserSubject major in this.keywordSubjectPair["전공전문"])
                {
                    foreach (SimillarMajor simillar in simillarList)
                    {
                        if (major.subjectName == simillar.currSubjectName)// 수강한 과목이 이전 전공명과 동일 할 경우(ex. 14년도 교육과정 적용 학생이 주니어디자인프로젝트가 아닌 공개sw수강)
                        {
                            // Console.WriteLine("교육과정 적용년도 " + major.);
                            //applicationYear 입력 받아야함
                            if (Convert.ToInt32(this.applicationYear) <= simillar.prevSubjectEndYear && Convert.ToInt32(this.applicationYear) >= simillar.prevSubjectStartYear)
                            {
                                //예외처리 오류 메시지 입력
                                exceptionList.Add(simillar.prevSubjectName + "과목이 동일유사전공교과목인 " + major.subjectName + " 으로 수강되었는지 확인하십시오.");
                            }
                        }
                    }
                }


                //타 전공 동일 유사 교과목 확인.

                foreach (List<UserSubject> subjectList in this.keywordSubjectPair.Values)
                {
                    foreach (UserSubject subject in subjectList)
                    {
                        foreach (DiffMajor different in diffMajorList)
                        {
                            if (subject.subjectCode == different.otherSubjectCode)//유사교과목이 수강 된 경우
                            {
                                foreach (UserSubject majorSubject in this.keywordSubjectPair["전공"])
                                {
                                    if (majorSubject.subjectCode == different.subjectCode)// 유사교과목과 동일한 전공 수강여부 확인
                                    {
                                        if (Convert.ToInt32(majorSubject.year) <= different.endYear && Convert.ToInt32(majorSubject.year) >= different.startYear)
                                        {
                                            exceptionList.Add(majorSubject.subjectName + "과목이 타과 동일유사교과목인 " + different.otherSubjectName + "과 중복 수강 되었습니다.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



    }
}