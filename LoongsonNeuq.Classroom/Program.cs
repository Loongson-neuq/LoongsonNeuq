﻿using LoongsonNeuq.Classroom;
using LoongsonNeuq.Common;
using LoongsonNeuq.Common.Auth;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddGitHubAuth()
    .WithToken<EnvTokenProvider>()
    .AddGitHubClient();

services.AddSingleton<StudentsTable>();

services.AddLogging();

var serviceProvider = services.BuildServiceProvider();

// TODO