using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevHabit.Api.Services;

public static class CustomMediaTypeNames
{
    public static class App
    {
        public const string Hateoas = "application/vnd.dev-habit.hateoas+json";
    }

}