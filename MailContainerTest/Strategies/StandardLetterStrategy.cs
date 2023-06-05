﻿using MailContainerTest.Abstractions;
using MailContainerTest.Types;

namespace MailContainerTest.Strategies;

public sealed class StandardLetterStrategy : IMailTransferStrategy
{
    public bool IsSuccess(MailContainer? sourceContainer, MailContainer? destContainer)
    {
        if (sourceContainer is null || destContainer is null)
        {
            return false;
        }
        
        if (!sourceContainer.AllowedMailType.HasFlag(AllowedMailType.StandardLetter) || !destContainer.AllowedMailType.HasFlag(AllowedMailType.StandardLetter))
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