﻿using System.Data.SqlClient;
using Bits.Domain;
using FizzWare.NBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Assertions;
using FluentNHibernate.Testing;
using FluentValidation;
using NHibernate.Exceptions;
using Xunit;

namespace Bits.IntegrationTests.Persistence.Maps
{
    public class TaskMapFacts : IUseFixture<DatabaseFixture>
    {
        private DatabaseFixture database;

        public void SetFixture(DatabaseFixture data)
        {
            database = data;
            database.UseLatestDatabaseSchema();
            database.LoadData(GetType());
        }

        [Fact]
        public void PersistEntityToDatabase()
        {
            using (var session = database.SessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var entity = Builder<Task>.CreateNew().Build();

                new PersistenceSpecification<Task>(session).VerifyTheMappings(entity);

                transaction.Rollback();
            }
        }

        [Fact]
        public void TaskStatusHydratesObjectWithCorrectValue()
        {
            using (var session = database.SessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var entity = session.Load<Task>(1);

                entity.Status.Should().Be(TaskStatus.InProgress);

                transaction.Rollback();
            }
        }

        [Fact]
        public void DuplicateItemsThrowsConstraintException()
        {
            using (var session = database.SessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var entity = Builder<Task>.CreateNew().With(x => x.Name = "Need more caffination").Build();

                Action act = () => session.Save(entity);

                act.ShouldThrow<GenericADOException>()
                   .WithInnerException<SqlException>()
                   .WithInnerMessage("Cannot insert duplicate key row in object 'dbo.Tasks' with unique index 'UX_Name'", ComparisonMode.EquivalentSubstring);

                transaction.Rollback();
            }
        }

        [Fact]
        public void EntityWithInvalidStateThrowsValidationException()
        {
            using (var session = database.SessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var entity = Builder<Task>.CreateNew().With(x => x.Name = null).Build();

                Action act = () => session.Save(entity);

                act.ShouldThrow<ValidationException>().WithMessage("'Name' should not be empty.",
                                                                   ComparisonMode.EquivalentSubstring);

                transaction.Rollback();
            }
        }
    }
}
