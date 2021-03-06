﻿using Bits.Domain;
using FluentNHibernate.Mapping;

namespace Bits.Persistence.Maps
{
    public class TaskMap : ClassMap<Task>
    {
        public TaskMap()
        {
            Table("Tasks");
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Name);

            Map(x => x.Priority).CustomType<TaskPriority>();
            Map(x => x.Status).CustomType<TaskStatus>();
        }
    }
}