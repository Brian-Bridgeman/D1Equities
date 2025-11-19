using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using D1Equities.Sim;

namespace D1Equities.GUI.Model
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set;}
        public string Password { get; set;}
        public string Name { get; set;}
        public string LastName { get; set;}
        public string Email { get; set;}
        
        public Portfolio? Portfolio { get; set;}
        //add portfolio property
        // portfolio property = portfolio.Load(Id)
    }
}
