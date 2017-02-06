﻿using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MediatR.Tests.Publishing
{
    public class NoHandlers
    {
        class ServiceLocator
        {
            private readonly Dictionary<Type, List<object>> Services = new Dictionary<Type, List<object>>();

            public void Register(Type type, params object[] implementations)
                => Services.Add(type, implementations.ToList());

            public List<object> Get(Type type) { return Services[type]; }
        }

        public class TasksList
        {
            public List<string> Tasks { get; }

            public TasksList(params string[] tasks)
            {
                Tasks = tasks.ToList();
            }
        }

        public class GetTaskNamesQuery : IRequest<List<string>>
        {
            public string Filter { get; }

            public GetTaskNamesQuery(string filter)
            {
                Filter = filter;
            }
        }

        public class GetTaskNamesQueryHandler : IRequestHandler<GetTaskNamesQuery, List<string>>
        {
            private readonly TasksList _taskList;
            public GetTaskNamesQueryHandler(TasksList tasksList)
            {
                _taskList = tasksList;
            }

            public List<string> Handle(GetTaskNamesQuery query)
            {
                return _taskList.Tasks
                    .Where(taskName => taskName.ToLower().Contains(query.Filter.ToLower()))
                    .ToList();
            }
        }

        public NoHandlers()
        {
        }

        [Fact]
        public async void GivenNonRegisteredQueryHandler_WhenPublishMethodIsBeingCalled_ThenThrowsAnError()
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                //Given
                var serviceLocator = new ServiceLocator();
                var mediator = new Mediator(
                        type => serviceLocator.Get(type).FirstOrDefault(),
                        type => serviceLocator.Get(type));

                var query = new GetTaskNamesQuery("cleaning");

                //When
                var result = await mediator.Send(query);
            });

            //Then
            ex.Should().Not.Be.Null();
        }
    }
}
