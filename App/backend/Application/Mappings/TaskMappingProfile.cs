using App.Application.Data;
using App.Domain.Model;
using AutoMapper;
using System;

namespace App.Application.MappingProfiles;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<UserTask, UserTaskDTO>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<UserTaskDTO, UserTask>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<TaskPriority>(src.Priority)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TaskStatus>(src.Status)));
    }
}
