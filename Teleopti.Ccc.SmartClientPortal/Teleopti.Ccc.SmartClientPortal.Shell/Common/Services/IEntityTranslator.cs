using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Services
{
    public interface IEntityTranslator
    {
        bool CanTranslate(Type targetType, Type sourceType);
        bool CanTranslate<TTarget, TSource>();
        object Translate(IEntityTranslatorService service, Type targetType, object source);
        TTarget Translate<TTarget>(IEntityTranslatorService service, object source);
    }
}