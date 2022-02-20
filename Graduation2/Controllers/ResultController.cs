using Graduation2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;

namespace Graduation2.Controllers
{
    public class ResultController : Controller
    {
        // private readonly ILogger<HomeController> _logger;
        
        private IWebHostEnvironment environment;
        public ResultController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        // public HomeController(ILogger<HomeController> logger)
        // {
        //     _logger = logger;
        // }
        public bool IsValidRule(Rule rule)
        {
          if(rule.division == "교양" || rule.division == "전공")
            return true;
          else
            return false;
          // if(Convert.ToInt32(newRule.sequenceNumber) > 23 || Convert.ToInt32(newRule.sequenceNumber) < 6)
          //    return false;
          // else
          //   return true;
        }
        public IActionResult Index()
        {
            // 한글 인코딩
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string baseFilePath = this.environment.WebRootPath;

            // string inputFile = Path.Combine(baseFilePath, "upload",fileNames[0]);
            string templateFilePath = Path.Combine(baseFilePath, "template_2016CSE.xlsx");
            string gradeFile = Path.Combine(baseFilePath, "student_score.xlsx");

            UserInfo userInfo = new UserInfo();
            userInfo.GetUserSubject(gradeFile); // 수강 과목 리스트 및 이수 학점

            List<Rule> rules = new List<Rule>();

            string enrollmentYear = "";
            string major = "";

            // ---- old ver. 템플릿 파싱 코드
            using (var stream = System.IO.File.Open(templateFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using(var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // sheet 
                    
                    int currentRuleNum = 0;
                    int currentSheetNum = 1;
                    List<int> multiInputRuleNumber = new List<int>();
                    string ruleDivision = "";
                    // will be passed to View
                    reader.Read();
                    while(reader.Read())
                    {
                        string[] valueArray = new string[6]; // 모두 string임에 주의
                        
                        for(int i = 0; i < 6; i++)
                        {
                            if (reader.GetValue(i) == null)
                                valueArray[i] = "";
                            else
                                valueArray[i] = reader.GetValue(i).ToString();
                        }
                        if(valueArray[0] == "" || valueArray[0] == null)
                          valueArray[0] = ruleDivision;
                        else
                          ruleDivision = valueArray[0];

                        // -- Rule Generator --
                        RuleBuilder ruleBuilder = new RuleBuilder();
                        Rule newRule = ruleBuilder.SetDivision(ruleDivision)
                                                  .SetSequenceNumber(valueArray[1])
                                                  .SetQuestion(valueArray[2])
                                                  .SetSingleInput(valueArray[3])
                                                  .SetReplyType(valueArray[4]) // cell order changed
                                                  .Build();
                        // DI..?
                        CheckStrategy checkStrategy = null;
                        // 처음 기초정보, 뒷부분 졸업요건 파트 키워드 적용 잘 안됨
                        if(!IsValidRule(newRule))
                          checkStrategy = new NoCheckStrategy();
                        else {
                          switch(newRule.replyType)
                          {
                            case "단수":
                              checkStrategy = new NumberValueChecker(userInfo);
                              break;
                            case "OX":
                              checkStrategy = new OXValueChecker(userInfo);
                              break;
                            case "목록":
                              checkStrategy = new MultiValueChecker(userInfo);
                              break;
                            default:
                              checkStrategy = new NoCheckStrategy();
                              break;
                          }
                        }

                        newRule.SetCheckStrategy(checkStrategy);

                        if(valueArray[4] == "목록")
                        {
                            multiInputRuleNumber.Add(currentRuleNum);
                        }
                        // 기초정보를 바탕으로 rule name 생성
                        if(newRule.division == "기초정보")
                        {
                          if (newRule.question.Contains("입학년도"))
                            enrollmentYear = newRule.singleInput;
                          else if (newRule.question.Contains("학과"))
                            major = newRule.singleInput;
                        }
                        // 실제 Rule 저장
                        rules.Add(newRule);
                        currentRuleNum++;
                    }

                    while(reader.NextResult()) // next sheet
                    {
                      List<Subject> newSubjects = new List<Subject>();
                      currentSheetNum++;
                      reader.Read();reader.Read();
                      
                      while(reader.Read())
                      {
                        // 전공 or 설계과목 : cols = 5
                        int cols = reader.FieldCount;

                        string[] valueArray = new string[cols];
                        for(int i = 0 ; i < cols ; i++)
                        {
                            if (reader.GetValue(i) == null)
                                valueArray[i] = "";
                            else
                                valueArray[i] = Regex.Replace(reader.GetValue(i).ToString(), @"\s", ""); // 과목명 내 띄어쓰기 제거
                        }
                        if (String.IsNullOrEmpty(valueArray[1])) break;
                        
                        if(!(valueArray[0].Contains("예시"))) // 대체인정 시트가 아닌경우만
                        {
                            Subject newSubject = new Subject{
                              subjectCode = valueArray[1],
                              subjectName = valueArray[2],
                              credit = Convert.ToInt32(valueArray[3].Trim()),
                              year = valueArray[4].Trim()
                            };
                            if(cols == 6) // 설계과목일 경우
                            {
                              newSubject.year = valueArray[cols-1];
                            }
                            newSubjects.Add(newSubject);
                        }
                      }
                  
                      int ruleIdx = multiInputRuleNumber[currentSheetNum-2];
                      rules[ruleIdx].requiredSubjects = newSubjects;
                    }
                }
            }
            //------------------------
            foreach(Rule rule in rules)
            {
              rule.CheckRule();
            }
            var result = new Tuple<UserInfo, List<Rule>>(userInfo, rules) {};
            return View(result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
