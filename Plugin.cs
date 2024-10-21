using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using PhoneApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Net.Http;
using Newtonsoft.Json;

namespace UserLoaderPlugin
{
    [PhoneApp.Domain.Attributes.Author(Name = "Ivan Petrov")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Запуск плагина загрузки пользователей");
            var employeesList = args.Cast<EmployeesDTO>().ToList();

            try
            {
                // Загрузка новых пользователей из API
                var newUsers = LoadUsersFromApi().GetAwaiter().GetResult();

                // Добавляем новых пользователей в список сотрудников
                foreach (var user in newUsers)
                {
                    var employee = new EmployeesDTO
                    {
                        Name = $"{user.firstName} {user.lastName}"
                    };
                    employee.AddPhone(user.phone);
                    employeesList.Add(employee);
                    logger.Info($"Добавлен новый пользователь: {employee.Name}, Телефон: {user.phone}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка при загрузке пользователей: {ex.Message}");
                logger.Trace(ex.StackTrace);
            }

            return employeesList.Cast<DataTransferObject>();
        }

        private async Task<List<UserDTO>> LoadUsersFromApi()
        {
            string apiUrl = "https://dummyjson.com/users"; // URL для загрузки данных
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync(apiUrl);
                    var result = JsonConvert.DeserializeObject<UserApiResponse>(response);
                    return result.users;
                }
                catch (HttpRequestException httpEx)
                {
                    logger.Error($"Ошибка HTTP при получении данных: {httpEx.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error($"Ошибка при обработке данных из API: {ex.Message}");
                    throw;
                }
            }
        }
    }

    // DTO классы для работы с API
    public class UserDTO
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
    }

    public class UserApiResponse
    {
        public List<UserDTO> users { get; set; }
    }
}
