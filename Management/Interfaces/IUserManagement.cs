using Management.Entities;
using System.Collections.Generic;

namespace Management.Interfaces
{
    public interface IUserManagement
    {
        IEnumerable<User> ListUsers();
    }
}
