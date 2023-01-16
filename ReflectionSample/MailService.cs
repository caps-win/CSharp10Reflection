using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReflectionSample
{
  public class MailService
  {
    public void SendEmail(string address, string subject)
    {
      Console.WriteLine($"Sending a warning mail to {address} with subject {subject}");
    }
  }
}