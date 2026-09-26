using System.Collections.Generic;
using TechStackLearningHub.Web.Data_Access_Layer;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.BLL
{
    public class UserBLL
    {
        private readonly UserDAL _userDAL = new UserDAL();

        public List<User> GetAllUsersForAdmin()
        {
            return _userDAL.SelectAll();
        }

        public void DeactivateUser(int userId)
        {
            _userDAL.UpdateStatus(userId, false);
        }

        public void ActivateUser(int userId)
        {
            _userDAL.UpdateStatus(userId, true);
        }

        public void ChangeUserRole(int userId, string newRoleName)
        {
            // Resolve by name rather than trusting a posted role id, so a
            // tampered <select> value can't assign an arbitrary RoleID.
            int roleId = _userDAL.GetRoleIdByName(newRoleName);
            if (roleId <= 0)
                throw new ValidationException("Choose a valid role.");
            _userDAL.UpdateRole(userId, roleId);
        }

        public int GetActiveStudentCount()
        {
            return _userDAL.GetActiveStudentCount();
        }
    }
}
