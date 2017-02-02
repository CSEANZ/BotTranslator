using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BotTranslator.Contract;
using BotTranslator.Services;

namespace BotTranslator.Glue
{
    public static class TranslatorGlue
    {
        public static IContainer Container { get; private set; }

        static TranslatorGlue()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<CognitiveServicesTranslator>().As<ICognitiveServicesTranslator>().SingleInstance();
            builder.RegisterType<TranslatorService>().As<ITranslatorService>().SingleInstance();

            Container = builder.Build();
        }
    }
}
