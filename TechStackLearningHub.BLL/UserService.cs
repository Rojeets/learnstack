using System.Data;
using TechStackLearningHub.DAL;

namespace TechStackLearningHub.BLL
{
    public class UserService
    {
        private readonly UserRepository _userRepository = new UserRepository();

        public DataTable GetAllUsersForAdmin()
        {
            return _userRepository.GetAllUsers();
        }

        public void DeactivateUser(int userId)
        {
            _userRepository.UpdateUserStatus(userId, false);
        }

        public void ActivateUser(int userId)
        {
            _userRepository.UpdateUserStatus(userId, true);
        }

        public void ChangeUserRole(int userId, string newRoleName)
        {
            // Resolve by name rather than trusting a posted role id, so a
            // tampered <select> value can't assign an arbitrary RoleID.
            int roleId = _userRepository.GetRoleIdByName(newRoleName);
            if (roleId <= 0)
                throw new System.InvalidOperationException("Unknown role: " + newRoleName);
            _userRepository.UpdateUserRole(userId, roleId);
        }

        public int GetActiveStudentCount()
        {
            return _userRepository.GetActiveStudentCount();
        }
    }
}