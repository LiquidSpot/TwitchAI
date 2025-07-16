using System.Text.Json;
using AutoMapper;
using TwitchAI.Application.Dto.Response;
using TwitchAI.Domain.Entites;

namespace TwitchAI.Application.Behaviors.MapperProfiles
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            /* ---------- Usage ---------- */
            CreateMap<UsageDto, Usage>()
               .ForMember(d => d.Id, m => m.Ignore())
               .ForMember(d => d.prompt_tokens, m => m.MapFrom(s => s.prompt_tokens))
               .ForMember(d => d.completion_tokens, m => m.MapFrom(s => s.completion_tokens))
               .ForMember(d => d.total_tokens, m => m.MapFrom(s => s.total_tokens));

            /* ---------- Choice ---------- */
            CreateMap<ChoiceDto, Choice>()
               .ForMember(d => d.Id, m => m.Ignore())
               .ForMember(d => d.Index, m => m.MapFrom(s => s.index))
               .ForMember(d => d.FinishReason, m => m.MapFrom(s => s.finish_reason))
               .ForMember(d => d.Text, m => m.MapFrom(s => s.message.content))
               .ForMember(d => d.Message, m => m.MapFrom(s => MappingHelpers.SerializeMessage(s.message)));

            /* ---------- TextCompletion ---------- */
            CreateMap<TextCompletionDto, TextCompletion>()
               .ForMember(d => d.Id, m => m.Ignore())
               .ForMember(d => d.GptId, m => m.MapFrom(s => s.id))
               .ForMember(d => d.@object, m => m.MapFrom(s => s.@object))
               .ForMember(d => d.model, m => m.MapFrom(s => s.model))
               .ForMember(d => d.created, m => m.MapFrom(s => s.created))

                // заполняем навигационные свойства
               .ForMember(d => d.Usage, m => m.MapFrom(s => s.usage))
               .ForMember(d => d.Choices, m => m.MapFrom(s => s.choices))

                // если не хотите писать UsageId вручную
               .ForMember(d => d.IdUsage, m => m.Ignore());
        }
    }
}
