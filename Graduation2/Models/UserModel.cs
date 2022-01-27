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
          TempRule temp = new TempRule();
          temp.keyword=reader["keyword"].ToString();
          temp.question_type=reader["question_type"].ToString();
          temp.value = reader["value"].ToString();
          //rule.Add(new TempRule(){reader["keyword"].ToString(),reader["question_type"].ToString(),reader["value"].ToString()});
          this.rule.Add(temp);
        }
      }
    }
  }
  
  }
}