using System.Collections.Generic;
using System.Data;
using TechStackLearningHub.DAL;

namespace TechStackLearningHub.BLL
{
    public class ModuleService
    {
        private readonly ModuleRepository _moduleRepository = new ModuleRepository();
        private readonly LessonService _lessonService = new LessonService();

        public DataTable GetModulesForCourse(int courseId)
        {
            return _moduleRepository.GetModulesByCourseId(courseId);
        }

        public int AddModuleToCourse(int courseId, string title)
        {
            DataTable existing = _moduleRepository.GetModulesByCourseId(courseId);
            int nextOrder = 1;
            foreach (DataRow row in existing.Rows)
            {
                int order = (int)row["ModuleOrder"];
                if (order >= nextOrder) nextOrder = order + 1;
            }
            return _moduleRepository.InsertModule(new DAL.Models.Module
            {
                CourseID = courseId,
                ModuleTitle = title,
                ModuleOrder = nextOrder
            });
        }

        public void RenameModule(int moduleId, string newTitle)
        {
            // Fetch first so ModuleOrder and CourseID are preserved and only
            // the title actually changes.
            DAL.Models.Module module = _moduleRepository.GetModuleById(moduleId);
            if (module == null)
                throw new System.InvalidOperationException("Module not found.");
            module.ModuleTitle = newTitle;
            _moduleRepository.UpdateModule(module);
        }

        public void ReorderModules(List<int> moduleIdsInNewOrder)
        {
            // Drag-and-drop reorder produces an ordered list of ids; there is
            // no single SQL statement that reorders an arbitrary list, so this
            // bounded loop assigns each its new sequence number.
            for (int index = 0; index < moduleIdsInNewOrder.Count; index++)
            {
                _moduleRepository.ReorderModule(moduleIdsInNewOrder[index], index + 1);
            }
        }

        public void DeleteModule(int moduleId)
        {
            _moduleRepository.DeleteModule(moduleId);
        }

        public bool CourseHasContent(int courseId)
        {
            DataTable modules = GetModulesForCourse(courseId);
            if (modules.Rows.Count == 0)
                return false;
            foreach (DataRow row in modules.Rows)
            {
                int moduleId = (int)row["ModuleID"];
                if (_lessonService.GetLessonsForModule(moduleId).Rows.Count > 0)
                    return true;
            }
            return false;
        }
    }
}