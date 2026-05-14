using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.Response
{
    /// <summary>
    /// Вспомогательный класс для удобного создания ответов
    /// </summary>
    public static class CreatorResponse
    {
        /// <summary>
        /// Успешный ответ с данными (200 OK)
        /// </summary>
        public static BaseResponse<T> Ok<T>(T data, string? description = null)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.OK,
                Data = data,
                Description = description ?? "Операция выполнена успешно"
            };
        }

        /// <summary>
        /// Ресурс успешно создан (201 Created)
        /// </summary>
        public static BaseResponse<T> Created<T>(T data, string? description = null)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.Created,
                Data = data,
                Description = description ?? "Ресурс успешно создан"
            };
        }

        public static BaseResponse<T> NoContent<T>(string? description = null)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.NoContent,
                Description = description ?? "Ресурс найден, но данные в нем отсутствуют"
            };
        }

        /// <summary>
        /// Ошибка: неверные данные (400 Bad Request)
        /// </summary>
        public static BaseResponse<T> BadRequest<T>(string description, T? data = default)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.BadRequest,
                Description = description,
                Data = data
            };
        }

        /// <summary>
        /// Ошибка: не авторизован (401 Unauthorized)
        /// </summary>
        public static BaseResponse<T> Unauthorized<T>(string? description = null)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.Unauthorized,
                Description = description ?? "Пользователь не авторизован"
            };
        }

        /// <summary>
        /// Ошибка: доступ запрещён (403 Forbidden)
        /// </summary>
        public static BaseResponse<T> Forbidden<T>(string? description = null)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.Forbidden,
                Description = description ?? "Доступ запрещён"
            };
        }

        /// <summary>
        /// Ошибка: ресурс не найден (404 Not Found)
        /// </summary>
        public static BaseResponse<T> NotFound<T>(string? description = null)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.NotFound,
                Description = description ?? "Ресурс не найден"
            };
        }

        /// <summary>
        /// Ошибка: конфликт данных (409 Conflict)
        /// </summary>
        public static BaseResponse<T> Conflict<T>(string description)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.Conflict,
                Description = description
            };
        }

        /// <summary>
        /// Ошибка: внутренняя ошибка сервера (500 Internal Server Error)
        /// </summary>
        public static BaseResponse<T> InternalError<T>(string? description = null)
        {
            return new BaseResponse<T>
            {
                StatusCode = StatusCode.InternalServerError,
                Description = description ?? "Внутренняя ошибка сервера"
            };
        }
    }
}
