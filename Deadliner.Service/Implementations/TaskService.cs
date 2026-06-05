using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Deadliner.Domain.Response;
using Deadliner.Domain.ViewModel.Dashboard;
using Deadliner.Domain.ViewModel.Tasks;
using Deadliner.Service.Interfaces;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace Deadliner.Service.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITagRepository _tagRepository;

        public TaskService(ITaskRepository taskRepository, ITagRepository tagRepository)
        {
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
        }

        public async Task<BaseResponse<bool>> CompleteAsync(int userId, int taskId)
        {
            try
            {
                // Получаем текущий вариант задачи
                var userTask = await _taskRepository.GetByIdAsync(taskId);

                if (userTask == null)
                    return CreatorResponse.NotFound<bool>("Задача не найдена");

                // Проверка, что задача принадлежит именно этому пользователю
                var userTag = await _tagRepository.GetByIdAsync(userTask.TagId);

                if (userTag == null)
                    return CreatorResponse.NotFound<bool>("Тег задачи не найден");

                if (userTag.UserId != userId)
                    return CreatorResponse.Conflict<bool>("Эта задача принадлежит другому пользователю");

                if (userTask.Status == TasksStatus.Completed)
                    return CreatorResponse.Conflict<bool>("Задача уже выполнена");

                userTask.Status = TasksStatus.Completed;
                userTask.CompletedAt = DateTime.Now;

                await _taskRepository.UpdateAsync(userTask);

                return CreatorResponse.Ok(true);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<TaskDetailViewModel>> CreateAsync(int userId, TaskCreateViewModel model)
        {
            try
            {
                // Проверка, есть ли уже такая задача в теге
                if (await _taskRepository.ExistsByTitleAsync(model.Title, model.TagId))
                    return CreatorResponse.Conflict<TaskDetailViewModel>("В данном теге уже есть задача с таким названием");

                // Получаем тег, к которой будет привязана эта задача
                var userTag = await _tagRepository.GetByIdAsync(model.TagId);

                if (userTag == null)
                    return CreatorResponse.NotFound<TaskDetailViewModel>("Тег, к которой привязана эта задача, не найден");

                if (userTag.UserId != userId)
                    return CreatorResponse.Conflict<TaskDetailViewModel>("Созданная задача не может принадлежать другому пользователю");

                var userTask = new UserTask
                {
                    Title = model.Title,
                    Description = model.Description,
                    Priority = model.Priority,
                    Status = TasksStatus.Active,
                    Deadline = model.Deadline,
                    TagId = model.TagId,
                };

                // Создаем задачу, id в userTask сам присвоится благодаря EF Core
                await _taskRepository.CreateAsync(userTask);

                return CreatorResponse.Ok(ToTaskDetail(userTask, ToTagInfo(userTag)));
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<TaskDetailViewModel>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int userId, int taskId)
        {
            try
            {
                var userTask = await _taskRepository.GetByIdAsync(taskId);

                if (userTask == null)
                    return CreatorResponse.NotFound<bool>("Задача для удаления не найдена");

                var userTag = await _tagRepository.GetByIdAsync(userTask.TagId);

                if (userTag == null)
                    return CreatorResponse.NotFound<bool>("Тег задачи не найден");

                if (userTag.UserId != userId)
                    return CreatorResponse.Conflict<bool>("Задача не может принадлежать другому пользователю");

                await _taskRepository.DeleteAsync(userTask);

                return CreatorResponse.Ok(true);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<TaskDetailViewModel>> GetByIdAsync(int userId, int taskId)
        {
            try
            {
                var userTask = await _taskRepository.GetByIdAsync(taskId);

                if (userTask == null)
                    return CreatorResponse.NotFound<TaskDetailViewModel>("Задача не найдена");

                var userTag = await _tagRepository.GetByIdAsync(userTask.TagId);

                if (userTag == null)
                    return CreatorResponse.NotFound<TaskDetailViewModel>("Тег задачи не найден");

                if (userTag.UserId != userId)
                    return CreatorResponse.Conflict<TaskDetailViewModel>("Нельзя посмотреть задачи другого пользователя");

                return CreatorResponse.Ok(ToTaskDetail(userTask, ToTagInfo(userTag)));
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<TaskDetailViewModel>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<DashboardViewModel>> GetDashboardAsync(int userId, int limit)
        {
            try
            {
                // Получаем все теги пользователя с количеством задач в них
                var allUserTags = await _tagRepository.GetByUserIdAsync(userId);

                if (allUserTags == null || !allUserTags.Any())
                    return CreatorResponse.NoContent<DashboardViewModel>("У вас нет тегов");

                // Получаем все задачи пользователя
                var allUserTasks = await _taskRepository.GetByUserIdAsync(userId);

                if (allUserTasks == null || !allUserTasks.Any())
                    return CreatorResponse.NoContent<DashboardViewModel>("У вас нет задач");

                return CreatorResponse.Ok(GetDashboard(allUserTags, allUserTasks, limit));
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<DashboardViewModel>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<TaskListViewModel>> GetFilteredAsync(int userId, TaskFilterViewModel filter)
        {
            try
            {
                var allUserTasks = await _taskRepository.GetByUserIdAsync(userId);
                var allUserTags = await _tagRepository.GetByUserIdAsync(userId);

                if (allUserTasks == null || !allUserTasks.Any())
                    return CreatorResponse.NoContent<TaskListViewModel>("У вас нет задач");

                TaskListViewModel? taskList = GetByFilter(allUserTasks, allUserTags, filter);

                if (taskList == null || taskList.TotalCount == 0 || taskList.Tasks == null || !taskList.Tasks.Any())
                    return CreatorResponse.NoContent<TaskListViewModel>("У вас нет задач, подходящих под данную фильтрацию");

                return CreatorResponse.Ok(taskList);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<TaskListViewModel>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<TaskDetailViewModel>> UpdateAsync(int userId, TaskEditViewModel model)
        {
            try
            {
                var userTask = await _taskRepository.GetByIdAsync(model.Id);

                if (userTask == null)
                    return CreatorResponse.NotFound<TaskDetailViewModel>("Задача не найдена");

                // Проверка дубликата названия, только если название изменилось
                if (model.Title != userTask.Title && await _taskRepository.ExistsByTitleAsync(model.Title, model.TagId))
                    return CreatorResponse.Conflict<TaskDetailViewModel>("Такая задача уже существует в этом теге");

                var userTag = await _tagRepository.GetByIdAsync(model.TagId);

                if (userTag == null)
                    return CreatorResponse.NotFound<TaskDetailViewModel>("Тег, к которому прикреплена задача, не найден");

                if (userTag.UserId != userId)
                    return CreatorResponse.Conflict<TaskDetailViewModel>("Эта задача не может принадлежать другому пользователю");

                userTask.Title = model.Title;
                userTask.Description = model.Description;
                userTask.Priority = model.Priority;
                userTask.Deadline = model.Deadline;
                userTask.Status = model.Status;
                userTask.TagId = model.TagId;

                if (model.Status == TasksStatus.Completed)
                    userTask.CompletedAt = userTask.CompletedAt ?? DateTime.Now;
                else
                    userTask.CompletedAt = null;

                await _taskRepository.UpdateAsync(userTask);

                return CreatorResponse.Ok(ToTaskDetail(userTask, ToTagInfo(userTag)));
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<TaskDetailViewModel>($"Ошибка: {ex.Message}");
            }
        }

        private static TaskListViewModel? GetByFilter(IEnumerable<UserTask> userTasks, IEnumerable<Tag> tags, TaskFilterViewModel filter)
        {
            IEnumerable<UserTask> resultFilter = userTasks;

            // Фильтр по тегу
            if (filter.TagId != null)
            {
                resultFilter = resultFilter.Where(t => t.TagId == filter.TagId);

                if (resultFilter == null || !resultFilter.Any())
                    return null;
            }

            // Фильтр по приоритету
            if (filter.Priority != null)
            {
                resultFilter = resultFilter.Where(t => t.Priority == filter.Priority);

                if (resultFilter == null || !resultFilter.Any())
                    return null;
            }

            // Фильтр по статусу
            if (filter.Status != null)
            {
                resultFilter = resultFilter.Where(t => t.Status == filter.Status);

                if (resultFilter == null || !resultFilter.Any())
                    return null;
            }

            // Поиск по названию (по подстроке)
            if (filter.SearchQuery != null)
            {
                resultFilter = resultFilter.Where(t => t.Title.Contains(filter.SearchQuery));

                if (resultFilter == null || !resultFilter.Any())
                    return null;
            }

            // Показать только просроченные
            if (filter.OverdueOnly ?? false)
            {
                resultFilter = resultFilter.Where(t => t.Status == TasksStatus.Active && t.Deadline != null && t.Deadline < DateTime.Now);

                if (resultFilter == null || !resultFilter.Any())
                    return null;
            }

            // Сортировка по направлению
            if (filter.SortBy != null)
            {
                // Получаем направление сортировки
                string sortDirection = filter?.SortDirection == "asc" ? "asc" : "desc" ?? "asc";

                // Получаем критерий, по которому будем сортировать
                string sortBy = filter.SortBy == "deadline" ? "deadline" : (filter.SortBy == "priority" ? "priority" : "created_at");

                if (sortBy == "deadline")
                    resultFilter = sortDirection == "asc" ? resultFilter.OrderBy(t => t.Deadline) : resultFilter.OrderByDescending(t => t.Deadline);
                else if (sortBy == "priority")
                    resultFilter = sortDirection == "asc" ? resultFilter.OrderBy(t => t.Priority) : resultFilter.OrderByDescending(t => t.Priority);
                else
                    resultFilter = sortDirection == "asc" ? resultFilter.OrderBy(t => t.CreatedAt) : resultFilter.OrderByDescending(t => t.CreatedAt);

                if (resultFilter == null || !resultFilter.Any())
                    return null;
            }

            return ToTaskList(resultFilter, tags);
        }

        /// <summary>
        /// Получить дашборд
        /// </summary>
        /// <param name="userTags">Все теги пользователя</param>
        /// <param name="userTasks">Все задачи пользователя</param>
        /// <param name="limit">Максимальное число задач</param>
        /// <returns>Дашборд</returns>
        private static DashboardViewModel GetDashboard(IEnumerable<Tag> userTags, IEnumerable<UserTask> userTasks, int limit)
        {
            // Активные задачи (не просроченные)
            var activeTasks = userTasks
                .Where(elem => elem.Status == TasksStatus.Active && (elem.Deadline == null || elem.Deadline > DateTime.Now));

            // Активные задачи (просроченные)
            var overdueTasks = userTasks
                .Where(elem => elem.Status == TasksStatus.Active && elem.Deadline != null && elem.Deadline < DateTime.Now);

            // Выполненные задачи
            var completedTasks = userTasks
                .Where(elem => elem.Status == TasksStatus.Completed);

            // Отмененные задачи
            var cancelledTasks = userTasks
                .Where(elem => elem.Status == TasksStatus.Cancelled);

            // Сортировка (нам нужно еще по дедлайнам их рассортировать, чтобы показывать ближайшие)
            var nearestTasks = userTasks
                .OrderBy(elem => elem?.Deadline ?? DateTime.MaxValue)
                .Take(limit);

            return new DashboardViewModel()
            {
                ActiveCount = activeTasks.Count(),
                OverdueCount = overdueTasks.Count(),
                CompletedCount = completedTasks.Count(),
                CancelledCount = cancelledTasks.Count(),
                TaskGroups = GetTaskGroupByTag(userTags, nearestTasks),
            };
        }

        /// <summary>
        /// Получить теги со списком задач в них для дашборда
        /// </summary>
        /// <param name="userTags">Коллекция тегов (все теги)</param>
        /// <param name="userTasks">Коллекция задач (уже отсортированные и в ограниченном количестве)</param>
        /// <returns>Теги со списками</returns>
        private static List<TaskGroupByTagViewModel> GetTaskGroupByTag(IEnumerable<Tag> userTags, IEnumerable<UserTask> userTasks)
        {
            var tagWithTaskJoin = userTags
                .Join(
                userTasks,
                tag => tag.Id,
                task => task.TagId,
                (tag, task) => new
                {
                    TagId = tag.Id,
                    TagName = tag.Name,
                    ColorHex = tag.ColorHex,

                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    Priority = task.Priority,
                    Status = task.Status,
                    Deadline = task.Deadline,
                    IsOverdue = task.Status == TasksStatus.Active && task.Deadline != null && task.Deadline < DateTime.Now,
                })
                .GroupBy(t => t.TagId);

            var listResult = new List<TaskGroupByTagViewModel>();

            foreach (var tag in tagWithTaskJoin)
            {
                var taskGroupBy = new TaskGroupByTagViewModel()
                {
                    TagId = tag.First().TagId,
                    TagName = tag.First().TagName,
                    ColorHex = tag.First().ColorHex,

                    Tasks = tag
                    .Select(elem => new TaskItemViewModel
                    {
                        Id = elem.TaskId,
                        Title = elem.TaskTitle,
                        Priority = elem.Priority,
                        Status = elem.Status,
                        Deadline = elem.Deadline,
                        IsOverdue = elem.IsOverdue,
                    }
                    )
                    .ToList(),
                };

                listResult.Add(taskGroupBy);
            }

            return listResult;
        }

        /// <summary>
        /// Переводит из коллекции задач и списков в TaskListViewModel
        /// </summary>
        /// <param name="userFilterTasks">Отфильтрованные задачи пользователя</param>
        /// <param name="userAllTags">Все теги пользователя</param>
        /// <returns>Модель списка задач</returns>
        private static TaskListViewModel ToTaskList(IEnumerable<UserTask> userFilterTasks, IEnumerable<Tag> userAllTags)
        {
            // Строим словарь тегов для быстрого поиска
            var tagDict = userAllTags.ToDictionary(t => t.Id);

            var tasks = userFilterTasks.Select(task =>
            {
                tagDict.TryGetValue(task.TagId, out var tag);
                return new TaskDetailViewModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Priority = task.Priority,
                    Status = task.Status,
                    Deadline = task.Deadline,
                    CompletedAt = task.CompletedAt,
                    CreatedAt = task.CreatedAt,
                    IsOverdue = task.Status == TasksStatus.Active && task.Deadline != null && task.Deadline < DateTime.Now,
                    Tag = tag != null ? new TagInfoViewModel
                    {
                        Id = tag.Id,
                        Name = tag.Name,
                        ColorHex = tag.ColorHex,
                    } : null!
                };
            }).ToList();

            return new TaskListViewModel
            {
                TotalCount = tasks.Count,
                Tasks = tasks
            };
        }

        private static TaskDetailViewModel ToTaskDetail(UserTask userTask, TagInfoViewModel tagInfo) =>
            new TaskDetailViewModel
            {
                Id = userTask.Id,
                Title = userTask.Title,
                Description = userTask.Description,
                Priority = userTask.Priority,
                Status = userTask.Status,
                Deadline = userTask.Deadline,
                CompletedAt = userTask.CompletedAt,
                CreatedAt = userTask.CreatedAt,
                Tag = tagInfo,
                IsOverdue = userTask.Status == TasksStatus.Active && userTask.Deadline != null && userTask.Deadline < DateTime.Now,
            };

        private static TagInfoViewModel ToTagInfo(Tag userTag) =>
            new TagInfoViewModel
            {
                Id = userTag.Id,
                Name = userTag.Name,
                ColorHex = userTag.ColorHex
            };
    }
}
