using System;
using System.Collections.Generic;
using System.Text;

namespace Fan.Accounts
{
    public interface ISecurityService
    {
        string GetSha256Hash(string input);
    }
}
