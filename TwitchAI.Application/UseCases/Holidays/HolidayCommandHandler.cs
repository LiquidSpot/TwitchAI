using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;
using TwitchAI.Application.Interfaces;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Application.UseCases.Holidays
{
    /// <summary>
    /// Обработчик команды для получения праздника дня
    /// </summary>
    internal class HolidayCommandHandler : ICommandHandler<HolidayCommand, LSResponse<string>>
    {
        private readonly IHolidayService _holidayService;
        private readonly IExternalLogger<HolidayCommandHandler> _logger;

        public HolidayCommandHandler(
            IHolidayService holidayService,
            IExternalLogger<HolidayCommandHandler> logger)
        {
            _holidayService = holidayService ?? throw new ArgumentNullException(nameof(holidayService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LSResponse<string>> Handle(HolidayCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(new { 
                Method = nameof(Handle),
                UserId = request.UserId,
                Date = request.Date
            });

            try
            {
                var result = new LSResponse<string>();
                var targetDate = request.Date ?? DateTime.Today;

                // Получаем праздник дня с переводом на русский язык
                var holidayMessage = await _holidayService.GetTodayHolidayTranslatedAsync(targetDate, cancellationToken);

                if (!string.IsNullOrEmpty(holidayMessage))
                {
                    _logger.LogInformation(new { 
                        Method = nameof(Handle),
                        UserId = request.UserId,
                        Date = targetDate,
                        Holiday = holidayMessage,
                        Status = "Success"
                    });

                    return result.Success(holidayMessage);
                }
                else
                {
                    var noHolidayMessage = $"📅 На сегодня ({targetDate:dd.MM.yyyy}) праздников не найдено. Но каждый день может быть праздничным! 🌟";
                    
                    _logger.LogInformation(new { 
                        Method = nameof(Handle),
                        UserId = request.UserId,
                        Date = targetDate,
                        Status = "NoHoliday"
                    });

                    return result.Success(noHolidayMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError, new { 
                    Method = nameof(Handle),
                    Status = "Exception",
                    UserId = request.UserId,
                    Date = request.Date,
                    Error = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

                return new LSResponse<string>().Error(BaseErrorCodes.OperationProcessError, "Произошла ошибка при получении праздника дня.");
            }
        }
    }
} 