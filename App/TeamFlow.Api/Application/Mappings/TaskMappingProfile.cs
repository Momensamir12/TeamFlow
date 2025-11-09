using App.Application.Data;
using App.Application.Dto;
using App.Domain.Model;
using AutoMapper;
using System;

namespace App.Application.MappingProfiles;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        // Entity -> DTO (cast enums to integers for frontend)
        CreateMap<UserTask, UserTaskDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => (int)src.Priority))  // ✅ Cast to int
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));      // ✅ Cast to int

        // DTO -> Entity (cast integers back to enums)
        CreateMap<UserTaskDto, UserTask>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => (TaskPriority)src.Priority))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (TaskStatus)src.Status));

        // CreateTaskDto -> Entity (direct mapping, already enums)
        CreateMap<CreateTaskDto, UserTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))  
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))  
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore()) 
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
