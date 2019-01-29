using BlackCross.Platform.Terminal.Views;
using log4net.Appender;
using log4net.Core;

namespace BlackCross.Platform.Terminal.Helpers
{
    /// <summary>
    /// log4net appender for our custom <see cref="GuiConsole"/>
    /// </summary>
    public class GuiConsoleAppender : AppenderSkeleton
    {
        private GuiConsole console;

        public GuiConsoleAppender(GuiConsole console)
        {
            this.console = console;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var logLevel = GuiConsoleLogLevel.Debug;
            if (loggingEvent.Level >= Level.Error)
            {
                logLevel = GuiConsoleLogLevel.Error;
            }
            else if (loggingEvent.Level >= Level.Warn)
            {
                logLevel = GuiConsoleLogLevel.Warn;
            }
            else if (loggingEvent.Level >= Level.Info)
            {
                logLevel = GuiConsoleLogLevel.Info;
            }

            console.WriteLine($"[{loggingEvent.Level.DisplayName}][{loggingEvent.LoggerName}]{loggingEvent.RenderedMessage}", logLevel);
        }
    }
}
