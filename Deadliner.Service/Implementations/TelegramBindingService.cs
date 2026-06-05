using Deadliner.DAL.Interfaces;
using Deadliner.Domain.DTO;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Response;
using Deadliner.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Service.Implementations
{
    public class TelegramBindingService : ITelegramBindingService
    {
        private readonly ITelegramBindingRepository _bindingRepository;

        public TelegramBindingService(ITelegramBindingRepository bindingRepository)
        {
            _bindingRepository = bindingRepository;
        }

        // Это для активации бота (то есть в БД нужно добавить привязку именно Telegram chat, а сам User уже привязан)
        public async Task<BaseResponse<bool>> ActivateAsync(string token, long chatId)
        {
            try
            {
                var binding = await _bindingRepository.GetByTokenAsync(token);

                if (binding == null)
                    return CreatorResponse.NotFound<bool>("Неверный токен");

                if (binding.CreatedAt.AddMinutes(15) < DateTime.Now)
                {
                    await _bindingRepository.DeleteAsync(binding);
                    return CreatorResponse.BadRequest<bool>("Срок действия токена истек");
                }

                if (binding.UserId == null)
                {
                    await _bindingRepository.DeleteAsync(binding);
                    return CreatorResponse.BadRequest<bool>("Привязка не завершена — пользователь не указан");
                }

                // Если этот chat_id уже привязан к другому пользователю, то удаляем старую привязку
                var existingBinding = await _bindingRepository.GetByChatIdAsync(chatId);

                if (existingBinding != null && existingBinding.Id != binding.Id)
                {
                    await _bindingRepository.DeleteAsync(existingBinding);
                }

                binding.BindCode = null;
                binding.TelegramChatId = chatId;

                await _bindingRepository.UpdateAsync(binding);

                return CreatorResponse.Ok(true);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<TelegramBindingDto>> GetStatusAsync(int userId)
        {
            try
            {
                var binding = await _bindingRepository.GetByUserIdAsync(userId);

                if (binding == null)
                    return CreatorResponse.Ok(new TelegramBindingDto
                    {
                        IsBound = false,
                        BindedAt = null
                    });

                return CreatorResponse.Ok(new TelegramBindingDto
                {
                    IsBound = binding.TelegramChatId != 0,
                    BindedAt = binding.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<TelegramBindingDto>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<int?>> GetUserIdByChatIdAsync(long chatId)
        {
            try
            {
                var binding = await _bindingRepository.GetByChatIdAsync(chatId);

                if (binding == null)
                    return CreatorResponse.NotFound<int?>("У вас нет привязки к Telegram аккаунту");

                return CreatorResponse.Ok(binding.UserId);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<int?>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<TelegramTokenDTO>> StartBindingAsync(int userId)
        {
            try
            {
                var existingBinding = await _bindingRepository.GetByUserIdAsync(userId);

                if (existingBinding != null)
                {
                    if (existingBinding.TelegramChatId != 0)
                        return CreatorResponse.Conflict<TelegramTokenDTO>("У вас уже есть активная привязка к Telegram");

                    await _bindingRepository.DeleteAsync(existingBinding);
                }

                var token = Guid.NewGuid().ToString("N");

                var botUsername = "convenient_deadliner_bot";
                var deepLink = $"https://t.me/{botUsername}?start={token}";

                var binding = new TelegramBinding
                {
                    UserId = userId,
                    BindCode = token,
                    TelegramChatId = 0,
                    CreatedAt = DateTime.Now
                };

                await _bindingRepository.CreateAsync(binding);

                return CreatorResponse.Ok(new TelegramTokenDTO
                {
                    Token = token,
                    DeepLink = deepLink,
                    ExpiresAt = DateTime.Now.AddMinutes(15)
                });
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<TelegramTokenDTO>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> UnbindAsync(int userId)
        {
            try
            {
                var binding = await _bindingRepository.GetByUserIdAsync(userId);

                if (binding == null)
                    return CreatorResponse.NotFound<bool>("У вас нет привязки к Telegram аккаунту");

                await _bindingRepository.DeleteAsync(binding);

                return CreatorResponse.Ok(true, "Привязка с Telegram аккаунтом успешно удалена");
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }
    }
}
