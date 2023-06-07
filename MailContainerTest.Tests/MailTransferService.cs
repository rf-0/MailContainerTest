﻿using FluentAssertions;
using MailContainerTest.Abstractions;
using MailContainerTest.Data;
using MailContainerTest.Services;
using MailContainerTest.Types;
using NSubstitute;
using Xunit;

namespace MailContainerTest.Tests;

public sealed class MailTransferServiceTests
{
    private readonly IMailContainerDataStoreFactory _mailContainerDataStoreFactory = Substitute.For<IMailContainerDataStoreFactory>();
    private readonly IMailContainerDataStore _mailContainerDataStore = Substitute.For<IMailContainerDataStore>();
    private readonly IMailTransferStrategyFactory _mailTransferStrategyFactory = Substitute.For<IMailTransferStrategyFactory>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILoggerAdapter<IMailTransferService> _loggerAdapter = Substitute.For<ILoggerAdapter<IMailTransferService>>();

    [Fact]
    public void MakeMailTransfer_ShouldReturnFalse_WhenStrategyNotSuccessful()
    {
        // Arrange
        var request = new MakeMailTransferRequest
                      {
                          SourceMailContainerNumber = "1",
                          DestinationMailContainerNumber = "2",
                          MailType = MailType.LargeLetter,
                          NumberOfMailItems = 1
                      };
        _mailContainerDataStoreFactory.CreateMailContainerDataStore()
                                      .Returns(new MailContainerDataStore());
        _mailTransferStrategyFactory.CreateMakeMailTransferStrategy(Arg.Any<MailType>())
                                    .IsSuccess(Arg.Any<MailContainer?>(), Arg.Any<MailContainer?>(), Arg.Any<MakeMailTransferRequest>())
                                    .Returns(false);

        var mailTransferService = new MailTransferService(_mailContainerDataStoreFactory, _mailTransferStrategyFactory, _unitOfWork, _loggerAdapter);

        // Act
        var result = mailTransferService.MakeMailTransfer(request);

        // Assert
        _unitOfWork.DidNotReceive().Commit();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void MakeMailTransfer_ShouldReturnTrue_WhenStrategyNotSuccessful()
    {
        // Arrange
        var request = new MakeMailTransferRequest
                      {
                          SourceMailContainerNumber = "1",
                          DestinationMailContainerNumber = "2",
                          MailType = MailType.LargeLetter,
                          NumberOfMailItems = 1
                      };
        var sourceContainer = new MailContainer
                              {
                                  AllowedMailType = AllowedMailType.LargeLetter,
                                  MailContainerNumber = "1",
                                  Status = MailContainerStatus.Operational
                              };
        var destContainer = new MailContainer
                            {
                                AllowedMailType = AllowedMailType.LargeLetter,
                                MailContainerNumber = "2",
                                Status = MailContainerStatus.Operational
                            };
        sourceContainer.IncreaseCapacity(100);
        destContainer.IncreaseCapacity(100);
        
        _mailContainerDataStoreFactory.CreateMailContainerDataStore()
                                      .Returns(_mailContainerDataStore);
        _mailContainerDataStore.GetMailContainer(Arg.Any<MailContainerNumber>())
                               .Returns(sourceContainer, destContainer);
        
        _mailTransferStrategyFactory.CreateMakeMailTransferStrategy(Arg.Any<MailType>())
                                    .IsSuccess(Arg.Any<MailContainer?>(), Arg.Any<MailContainer?>(), Arg.Any<MakeMailTransferRequest>())
                                    .Returns(true);

        var mailTransferService = new MailTransferService(_mailContainerDataStoreFactory, _mailTransferStrategyFactory, _unitOfWork, _loggerAdapter);

        // Act
        var result = mailTransferService.MakeMailTransfer(request);

        // Assert
        _unitOfWork.Received(1).Commit();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void MakeMailTransfer_ShouldReturnFalse_WhenCommitNotSuccessful()
    {
        // Arrange
        var request = new MakeMailTransferRequest
                      {
                          SourceMailContainerNumber = "1",
                          DestinationMailContainerNumber = "2",
                          MailType = MailType.LargeLetter,
                          NumberOfMailItems = 1
                      };
        var sourceContainer = new MailContainer
                              {
                                  AllowedMailType = AllowedMailType.LargeLetter,
                                  MailContainerNumber = "1",
                                  Status = MailContainerStatus.Operational
                              };
        var destContainer = new MailContainer
                            {
                                AllowedMailType = AllowedMailType.LargeLetter,
                                MailContainerNumber = "2",
                                Status = MailContainerStatus.Operational
                            };
        sourceContainer.IncreaseCapacity(100);
        destContainer.IncreaseCapacity(100);
        _mailContainerDataStoreFactory.CreateMailContainerDataStore()
                                      .Returns(_mailContainerDataStore);
        _mailContainerDataStore.GetMailContainer(Arg.Any<MailContainerNumber>())
                               .Returns(sourceContainer, destContainer);
        
        _mailTransferStrategyFactory.CreateMakeMailTransferStrategy(Arg.Any<MailType>())
                                    .IsSuccess(sourceContainer, destContainer, Arg.Any<MakeMailTransferRequest>())
                                    .Returns(true);

        var mailTransferService = new MailTransferService(_mailContainerDataStoreFactory, _mailTransferStrategyFactory, _unitOfWork, _loggerAdapter);

        _unitOfWork.When(static x => x.Commit()).Throw<Exception>();

        // Act
        var result = mailTransferService.MakeMailTransfer(request);

        // Assert
        _unitOfWork.Received(1).Commit();
        _unitOfWork.Received(1).Rollback();
        _loggerAdapter.Received(1).LogError(Arg.Any<Exception>(), "Error saving changes to containers");
        result.Success.Should().BeFalse();
    }
}