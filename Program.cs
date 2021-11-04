using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace fastpath
{
  class Program
  {
    static void Main(string[] args)
    {
      var task = Run();

      task.Wait();
      File.WriteAllText("result.json", JsonSerializer.Serialize(task.Result));
    }

    static async Task<List<string>> Run()
    {
      var api = new API();

      Console.WriteLine("getting json");
      var usersJson = await api.Get("users.json");
      var userRolesJson = await api.Get("userroles.json");

      Console.WriteLine("deserializing json");
      var users = await JsonSerializer.DeserializeAsync<List<User>>(usersJson);
      var userRoles = await JsonSerializer.DeserializeAsync<List<UserRole>>(userRolesJson);

      Console.WriteLine("filtering users");
      return users.FindAll(user =>
            {
              var userRole = userRoles.Find(ur => ur.UserPrincipalName.Equals(user.UserPrincipalName));

              try
              {
                return user.DisplayName.ToLower().Contains('s') &&
                  user.AccountEnabled &&
                  (userRole.RoleName.Equals("Sales") || userRole.RoleName.Equals("Development"));
              }
              catch (Exception)
              {
                return false;
              }
            }
        )
        .ConvertAll<string>(user => user.DisplayName);
    }
  }

  class API
  {
    private static readonly HttpClient client = new HttpClient();
    private static readonly string baseUrl = "https://alexdmeyer.com/codetest/";

    public API()
    {
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public Task<Stream> Get(string file)
    {
      return client.GetStreamAsync(baseUrl + file);
    }
  }

  public class BaseUser
  {
    public string UserPrincipalName { get; set; }
  }

  public class User : BaseUser
  {
    public string DisplayName { get; set; }
    public bool AccountEnabled { get; set; }
  }

  public class UserRole : BaseUser
  {
    public string RoleName { get; set; }
  }

  public class UserResult
  {
    public string DisplayName { get; set; }

    public UserResult(string displayName)
    {
      DisplayName = displayName;
    }
  }
}
