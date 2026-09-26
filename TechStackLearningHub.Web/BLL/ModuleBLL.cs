using System.Collections.Generic;
using TechStackLearningHub.Web.Data_Access_Layer;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.BLL
{
    public class ModuleBLL
    {
        private readonly ModuleDAL _moduleDAL = new ModuleDAL();
        private readonly LessonBLL _lessonBLL = new LessonBLL();

        public List<Module> GetModulesForCourse(int courseId)
        {
            return _moduleDAL.SelectByCourseId(courseId);
        }

        public Module GetModuleById(int moduleId)
        {
            return _moduleDAL.SelectById(moduleId);
        }

        public int AddModuleToCourse(int courseId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Module title is required.");

            List<Module> existing = _moduleDAL.SelectByCourseId(courseId);
            int nextOrder = 1;
            foreach (Module module in existing)
            {
                if (module.ModuleOrder >= nextOrder) nextOrder = module.ModuleOrder + 1;
            }
            return _moduleDAL.Insert(new Module
            {
                CourseID = courseId,
                ModuleTitle = title,
                ModuleOrder = nextOrder
            });
        }

        public void RenameModule(int moduleId, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ValidationException("Module title is required.");

            // Fetch first so ModuleOrder and CourseID are preserved and only
            // the title actually changes.
            Module module = _moduleDAL.SelectById(moduleId);
            if (module == null)
                throw new ValidationException("Module not found.");
            module.ModuleTitle = newTitle;
            _moduleDAL.Update(module);
        }

        public void ReorderModules(List<int> moduleIdsInNewOrder)
        {
            // Drag-and-drop reorder produces an ordered list of ids; there is
            // no single SQL statement that reorders an arbitrary list, so this
            // bounded loop assigns each its new sequence number.
            for (int index = 0; index < moduleIdsInNewOrder.Count; index++)
            {
                _moduleDAL.Reorder(moduleIdsInNewOrder[index], index + 1);
            }
        }

        public void DeleteModule(int moduleId)
        {
            _moduleDAL.Delete(moduleId);
        }

        public bool CourseHasContent(int courseId)
        {
            List<Module> modules = GetModulesForCourse(courseId);
            foreach (Module module in modules)
            {
                if (_lessonBLL.GetLessonsForModule(module.ModuleID).Count > 0)
                    return true;
            }
            return false;
        }
    }
}
