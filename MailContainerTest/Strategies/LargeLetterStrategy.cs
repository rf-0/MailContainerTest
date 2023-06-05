﻿using MailContainerTest.Abstractions;
using MailContainerTest.Types;

namespace MailContainerTest.Strategies;

public sealed class LargeLetterStrategy : IMailTransferStrategy
{
    public bool IsSuccess(MailContainer? sourceContainer, MailContainer? destContainer)
    {
        if (sourceContainer is null || destContainer is null)
        {
            return false;
        }
        
        if (!sourceContainer.AllowedMailType.HasFlag(AllowedMailType.LargeLetter) || !destContainer.AllowedMailType.HasFlag(AllowedMailType.LargeLetter))
        {
            return false;
        }
        
        if (sourceContainer.Status != MailContainerStatus.Operational || destContainer.Status != MailContainerStatus.Operational)
        {
            return false;
        }

        return true;
    }
}