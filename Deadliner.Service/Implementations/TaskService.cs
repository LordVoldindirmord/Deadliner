using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Deadliner.Domain.Response;
using Deadliner.Domain.ViewModel.Dashboard;
using Deadliner.Domain.ViewModel.Tasks;
using Deadliner.Service.Interfaces;
using System.Globalization;
using System.Net.NetworkInformation;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        public async Task<BaseResponse<byte[]>> GeneratePdfAsync(int userId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var from = fromDate.Date;
                var to = toDate.Date;

                if (from > to)
                    return CreatorResponse.BadRequest<byte[]>("Дата начала не может быть позже даты окончания");

                if (to > from.AddYears(1))
                    return CreatorResponse.BadRequest<byte[]>("Период не может превышать 1 год");

                var data = await GetPdfDataAsync(userId);
                var pdf = GeneratePlannerPdf(from, to, data);
                return CreatorResponse.Ok(pdf);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<byte[]>($"Ошибка при создании PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Получить данные для PDF файла
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Список задач</returns>
        private async Task<List<TaskPdfRow>> GetPdfDataAsync(int userId)
        {
            var tasks = await _taskRepository.GetByUserIdAsync(userId);
            var tags = await _tagRepository.GetByUserIdAsync(userId);
            var tagDict = tags.ToDictionary(t => t.Id);

            return tasks
                .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                .Select(t => new TaskPdfRow
                {
                    Title = t.Title,
                    TagName = tagDict.TryGetValue(t.TagId, out var tag) ? tag.Name : "—",
                    TagColorHex = tagDict.TryGetValue(t.TagId, out var tagColor) ? tagColor.ColorHex : "#6b7280",
                    Priority = FormatPriority(t.Priority),
                    Status = t.Status switch
                    {
                        TasksStatus.Active => (t.Deadline != null && t.Deadline < DateTime.Now) ? "Просрочена" : "Активна",
                        TasksStatus.Completed => "Выполнена",
                        TasksStatus.Cancelled => "Отменена",
                        _ => t.Status.ToString()
                    },
                    Deadline = t.Deadline?.ToString("dd.MM.yyyy") ?? "—",
                    DeadlineDate = t.Deadline?.Date
                })
                .ToList();
        }

        private static string FormatPriority(PriorityStatus priority) => priority switch
        {
            PriorityStatus.High => "Высокий",
            PriorityStatus.Medium => "Средний",
            PriorityStatus.Low => "Низкий",
            _ => priority.ToString()
        };

        private static readonly Color PlannerPrimary = Color.FromHex("#4a6cf7");
        private static readonly Color PlannerPrimaryDark = Color.FromHex("#3b5de7");
        private static readonly Color PlannerWeekend = Color.FromHex("#9333ea");
        private static readonly Color PlannerSaturday = Color.FromHex("#0891b2");
        private static readonly Color PlannerPageBg = Color.FromHex("#f4f6fb");
        private static readonly Color PlannerCardBg = Color.FromHex("#ffffff");
        private static readonly Color PlannerNotesBg = Color.FromHex("#fffbeb");
        private static readonly Color PlannerUndated = Color.FromHex("#f59e0b");

        /// <summary>
        /// Сгенерировать PDF-планер
        /// </summary>
        private static byte[] GeneratePlannerPdf(DateTime fromDate, DateTime toDate, List<TaskPdfRow> rows)
        {
            var culture = new CultureInfo("ru-RU");
            var days = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(i => fromDate.AddDays(i))
                .ToList();

            var tasksByDay = rows
                .Where(r => r.DeadlineDate.HasValue
                            && r.DeadlineDate.Value >= fromDate
                            && r.DeadlineDate.Value <= toDate)
                .GroupBy(r => r.DeadlineDate!.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Title).ToList());

            var undatedTasks = rows
                .Where(r => !r.DeadlineDate.HasValue && r.Status is "Активна" or "Просрочена")
                .OrderBy(t => t.Title)
                .ToList();

            var periodText = $"{fromDate.ToString("dd MMMM yyyy", culture)} — {toDate.ToString("dd MMMM yyyy", culture)}";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.Background().Background(PlannerPageBg);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(Color.FromHex("#374151")));

                    page.Header().Column(header =>
                    {
                        header.Item().AlignCenter().Text("Deadliner")
                            .FontSize(10).SemiBold().FontColor(PlannerPrimary);
                        header.Item().PaddingTop(2).Text("Планер")
                            .FontSize(22).Bold().AlignCenter().FontColor(Color.FromHex("#111827"));
                        header.Item().PaddingTop(4).Text(periodText)
                            .FontSize(11).AlignCenter().FontColor(Color.FromHex("#6b7280"));
                        header.Item().PaddingTop(8).Height(3).Background(PlannerPrimary);
                    });

                    page.Content().PaddingTop(14).Column(content =>
                    {
                        content.Spacing(12);

                        foreach (var day in days)
                        {
                            var dayTasks = tasksByDay.GetValueOrDefault(day) ?? [];
                            content.Item().Element(c => RenderDayBlock(c, day, dayTasks, culture));
                        }

                        if (undatedTasks.Count > 0)
                            content.Item().Element(c => RenderUndatedBlock(c, undatedTasks));
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Color.FromHex("#9ca3af")));
                        text.Span("Страница ");
                        text.CurrentPageNumber().SemiBold().FontColor(PlannerPrimary);
                        text.Span(" из ");
                        text.TotalPages().SemiBold().FontColor(PlannerPrimary);
                    });
                });
            }).GeneratePdf();
        }

        private static Color GetDayAccentColor(DayOfWeek dayOfWeek) => dayOfWeek switch
        {
            DayOfWeek.Saturday => PlannerSaturday,
            DayOfWeek.Sunday => PlannerWeekend,
            _ => PlannerPrimary
        };

        private static void RenderDayBlock(IContainer container, DateTime day, List<TaskPdfRow> tasks, CultureInfo culture)
        {
            var dayName = culture.DateTimeFormat.GetDayName(day.DayOfWeek);
            var capitalizedDayName = char.ToUpper(dayName[0]) + dayName[1..];
            var accent = GetDayAccentColor(day.DayOfWeek);
            var isWeekend = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            container
                .PreventPageBreak()
                .Background(PlannerCardBg)
                .Border(1).BorderColor(Color.FromHex("#e5e7eb"))
                .Column(dayCol =>
                {
                    dayCol.Item().Row(headerRow =>
                    {
                        headerRow.ConstantItem(5).Background(accent);

                        headerRow.RelativeItem().Background(accent).Padding(10).Row(header =>
                        {
                            header.RelativeItem().Column(col =>
                            {
                                col.Item().Text(capitalizedDayName)
                                    .FontSize(13).Bold().FontColor(Colors.White);
                                col.Item().Text(day.ToString("dd MMMM yyyy", culture))
                                    .FontSize(9).FontColor(Colors.White);
                            });

                            if (tasks.Count > 0)
                            {
                                header.ConstantItem(56).AlignMiddle().Background(Colors.White)
                                    .PaddingVertical(4).PaddingHorizontal(8)
                                    .Text($"{tasks.Count}")
                                    .FontSize(10).Bold().FontColor(accent).AlignCenter();
                            }
                            else if (isWeekend)
                            {
                                header.ConstantItem(56).AlignMiddle()
                                    .Text("Выходной")
                                    .FontSize(8).FontColor(Colors.White);
                            }
                        });
                    });

                    if (tasks.Count > 0)
                    {
                        for (var i = 0; i < tasks.Count; i++)
                        {
                            var index = i;
                            dayCol.Item().Element(c => RenderTaskRow(c, tasks[index], index % 2 == 1));
                        }
                    }
                    else
                    {
                        dayCol.Item().Padding(12).MinHeight(28).AlignCenter().AlignMiddle()
                            .Text("Свободный день")
                            .FontSize(9).Italic().FontColor(Color.FromHex("#9ca3af"));
                    }

                    dayCol.Item().Background(PlannerNotesBg).Padding(10).Column(notes =>
                    {
                        notes.Item().Text("Заметки")
                            .FontSize(8).SemiBold().FontColor(Color.FromHex("#d97706"));
                        notes.Item().PaddingTop(6).Height(22).BorderBottom(1)
                            .BorderColor(Color.FromHex("#fcd34d"));
                    });
                });
        }

        private static void RenderUndatedBlock(IContainer container, List<TaskPdfRow> tasks)
        {
            container
                .PreventPageBreak()
                .Background(PlannerCardBg)
                .Border(1).BorderColor(Color.FromHex("#e5e7eb"))
                .Column(block =>
                {
                    block.Item().Row(headerRow =>
                    {
                        headerRow.ConstantItem(5).Background(PlannerUndated);
                        headerRow.RelativeItem().Background(PlannerUndated).Padding(10)
                            .Text("Задачи без дедлайна")
                            .FontSize(13).Bold().FontColor(Colors.White);
                    });

                    for (var i = 0; i < tasks.Count; i++)
                    {
                        var index = i;
                        block.Item().Element(c => RenderTaskRow(c, tasks[index], index % 2 == 1));
                    }
                });
        }

        private static void RenderTaskRow(IContainer container, TaskPdfRow task, bool alternateRow)
        {
            var isCompleted = task.Status == "Выполнена";
            var isCancelled = task.Status == "Отменена";
            var isOverdue = task.Status == "Просрочена";
            var checkbox = isCompleted ? "☑" : "☐";
            var tagColor = ParseHexColor(task.TagColorHex, Color.FromHex("#6b7280"));
            var rowBg = alternateRow ? Color.FromHex("#f9fafb") : PlannerCardBg;

            container.Background(rowBg).PaddingHorizontal(10).PaddingVertical(6).Row(row =>
            {
                row.ConstantItem(16).AlignMiddle().Text(checkbox)
                    .FontSize(11).FontColor(isCompleted ? Color.FromHex("#10b981") : PlannerPrimary);

                row.ConstantItem(54).AlignMiddle().Element(c => RenderPriorityBadge(c, task.Priority));

                row.RelativeItem().AlignMiddle().Text(task.Title)
                    .FontSize(9)
                    .Strikethrough(isCompleted || isCancelled)
                    .FontColor(isCancelled ? Color.FromHex("#9ca3af") : Color.FromHex("#111827"));

                row.ConstantItem(78).AlignMiddle().Row(tagRow =>
                {
                    tagRow.ConstantItem(8).Height(8).Background(tagColor);
                    tagRow.RelativeItem().PaddingLeft(4).Text(task.TagName)
                        .FontSize(8).FontColor(Color.FromHex("#6b7280"));
                });

                if (isOverdue || isCompleted || isCancelled)
                {
                    row.ConstantItem(58).AlignMiddle().AlignRight()
                        .Element(c => RenderStatusBadge(c, task.Status));
                }
            });
        }

        private static void RenderPriorityBadge(IContainer container, string priority)
        {
            var (bg, fg, label) = priority switch
            {
                "Высокий" => (Color.FromHex("#fde8e8"), Color.FromHex("#ef4444"), "Выс."),
                "Средний" => (Color.FromHex("#fef3c7"), Color.FromHex("#d97706"), "Сред."),
                _ => (Color.FromHex("#f3f4f6"), Color.FromHex("#6b7280"), "Низк.")
            };

            container.Background(bg).PaddingVertical(2).PaddingHorizontal(4)
                .Text(label).FontSize(7).SemiBold().FontColor(fg);
        }

        private static void RenderStatusBadge(IContainer container, string status)
        {
            var (bg, fg) = status switch
            {
                "Просрочена" => (Color.FromHex("#fde8e8"), Color.FromHex("#ef4444")),
                "Выполнена" => (Color.FromHex("#d1fae5"), Color.FromHex("#059669")),
                _ => (Color.FromHex("#f3f4f6"), Color.FromHex("#9ca3af"))
            };

            container.Background(bg).PaddingVertical(2).PaddingHorizontal(4)
                .Text(status).FontSize(7).SemiBold().FontColor(fg);
        }

        private static Color ParseHexColor(string? hex, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return fallback;

            var normalized = hex.TrimStart('#');
            if (normalized.Length != 6)
                return fallback;

            try
            {
                return Color.FromHex(normalized);
            }
            catch
            {
                return fallback;
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
