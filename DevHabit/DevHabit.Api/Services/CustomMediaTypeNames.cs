using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevHabit.Api.Services;

public static class CustomMediaTypeNames
{
    public static class App
    {
        public const string HateoasV1 = "application/vnd.dev-habit.hateoas.1+json";
        public const string HateoasV2 = "application/vnd.dev-habit.hateoas.2+json";
        public const string JsonV1 = "application/json;v=1";
        public const string JsonV2 = "application/json;v=2";
    }

}