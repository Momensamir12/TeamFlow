using App.Application.DTOs;
using App.Domain.Model;
using AutoMapper;

namespace App.Application.MappingProfiles;

public class TaskCommentMappingProfile : Profile
{
    public TaskCommentMappingProfile()
    {
        // Entity -> DTO
        CreateMap<TaskComment, TaskCommentDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));

        // CreateTaskCommentDto -> Entity (constructor will be called)
        CreateMap<CreateTaskCommentDto, TaskComment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Task, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}
