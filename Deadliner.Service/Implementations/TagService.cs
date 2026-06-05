using Deadliner.DAL.Interfaces;
using Deadliner.DAL.Repositories;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Response;
using Deadliner.Domain.ViewModel.Tags;
using Deadliner.Service.Interfaces;

namespace Deadliner.Service.Implementations
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ITaskRepository _taskRepository;

        public TagService(ITagRepository tagRepository, ITaskRepository taskRepository)
        {
            _tagRepository = tagRepository;
            _taskRepository = taskRepository;
        }

        public async Task<BaseResponse<TagViewModel>> GetByIdAsync(int userId, int tagId)
        {
            try
            {
                var tag = await _tagRepository.GetByIdAsync(tagId);

                if(tag == null)
                    return CreatorResponse.NotFound<TagViewModel>("Тег не найден");

                if (tag.UserId != userId)
                    return CreatorResponse.Forbidden<TagViewModel>("Нет доступа к этому тегу");

                var tasksCount = (await _taskRepository.GetByTagIdAsync(tag.Id))?.Count() ?? 0;

                return CreatorResponse.Ok(ToTagViewModel(tag, tasksCount));
            }
            catch(Exception ex)
            {
                return CreatorResponse.InternalError<TagViewModel>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> CreateAsync(int userId, TagCreateViewModel model)
        {
            try
            {
                // Проверяем, есть ли тег уже у этого пользователя
                var TagIsExist = await _tagRepository.ExistsByNameAsync(model.Name, userId);

                if (TagIsExist)
                    return CreatorResponse.Conflict<bool>("Такой тег уже существует");

                // Создаем тег и записываем его в БД
                var newTag = new Tag()
                {
                    Name = model.Name,
                    ColorHex = model.ColorHex,
                    UserId = userId,
                };

                await _tagRepository.CreateAsync(newTag);

                return CreatorResponse.Created(true);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int userId, int tagId)
        {
            try
            {
                var deleteTag = await _tagRepository.GetByIdAsync(tagId);

                // Проверка существования тегов пользователя
                if (deleteTag == null)
                    return CreatorResponse.NotFound<bool>("Тег не найден");

                if (deleteTag.UserId != userId)
                    return CreatorResponse.Conflict<bool>("Вы не можете удалить чужой тег");

                // Все хорошо, удаляем
                await _tagRepository.DeleteAsync(deleteTag);

                return CreatorResponse.Ok(true);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<IEnumerable<TagViewModel>>> GetByUserIdAsync(int userId)
        {
            try
            {
                var tagsWithCounts = await _tagRepository.GetByUserIdWithTaskCountAsync(userId);

                // Проверка на то, что теги вообще есть
                if (tagsWithCounts == null || !tagsWithCounts.Any())
                    return CreatorResponse.NoContent<IEnumerable<TagViewModel>>("У пользователя нет тегов");

                var tagsListViewModel = tagsWithCounts
                    .Select(el => ToTagViewModel(el.tag, el.taskCount))
                    .ToList();

                return CreatorResponse.Ok<IEnumerable<TagViewModel>>(tagsListViewModel);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<IEnumerable<TagViewModel>>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<TagViewModel>> UpdateAsync(int userId, TagEditViewModel model)
        {
            try
            {
                // Старый тег
                var tag = await _tagRepository.GetByIdAsync(model.Id);

                // Проверка валидности
                if (tag == null)
                    return CreatorResponse.NotFound<TagViewModel>("Тег не найден");

                if (tag.UserId != userId)
                    return CreatorResponse.Conflict<TagViewModel>("Вы не можете редактировать чужой тег");

                if (model.Name != tag.Name && await _tagRepository.ExistsByNameAsync(model.Name, userId))
                    return CreatorResponse.Conflict<TagViewModel>("Тег с таким названием уже существует");

                tag.Name = model.Name;
                tag.ColorHex = model.ColorHex;

                await _tagRepository.UpdateAsync(tag);

                var tasks = await _taskRepository.GetByTagIdAsync(tag.Id);
                var taskCount = tasks.Count();

                return CreatorResponse.Ok(ToTagViewModel(tag, taskCount));
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<TagViewModel>($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Перевод из сущности БД в ViewModel
        /// </summary>
        /// <param name="tag">Модель сущности базы данных Tag</param>
        /// <param name="activeTaskCount">Количество задач у данного тега</param>
        /// <returns></returns>
        private TagViewModel ToTagViewModel(Tag tag, int activeTaskCount) =>
            new TagViewModel
            {
                Id = tag.Id,
                Name = tag.Name,
                ColorHex = tag.ColorHex,
                ActiveTaskCount = activeTaskCount,
            };
    }
}
