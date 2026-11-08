using API.Entities;
using API.Exceptions;
using API.Interfaces;
using API.Repositories;
using API.Services;
using Moq;
using NUnit.Framework;

namespace API.Tests.Unit;

[TestFixture]
public class ResourceServiceTests
{
    private Mock<IResourceRepository> _resources = null!;
    private Mock<IBookingRepository> _bookings = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private ResourceService _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _resources = new Mock<IResourceRepository>();
        _bookings = new Mock<IBookingRepository>();
        _currentUser = new Mock<ICurrentUserService>();
        _currentUser.SetupGet(x => x.UserId).Returns(_userId);
        _sut = new ResourceService(_resources.Object, _bookings.Object, _currentUser.Object);
    }

    [TestCase("")]
    [TestCase(" ")]
    public void CreateResource_WhenNameIsBlank_ThrowsBadRequest(string name)
    {
        Assert.That(
            async () => await _sut.CreateResource(name),
            Throws.TypeOf<BadRequestException>());

        _resources.Verify(x => x.AddAsync(It.IsAny<BookableResource>()), Times.Never);
    }

    [Test]
    public async Task CreateResource_TrimsNameAndStoresCurrentOwner()
    {
        BookableResource? created = null;
        _resources.Setup(x => x.AddAsync(It.IsAny<BookableResource>()))
            .Callback<BookableResource>(resource => created = resource)
            .Returns(Task.CompletedTask);

        var id = await _sut.CreateResource("  Meeting room  ");

        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Name, Is.EqualTo("Meeting room"));
        Assert.That(created.OwnerId, Is.EqualTo(_userId));
    }

    [Test]
    public void UpdateResource_WhenOwnerDoesNotMatch_ThrowsForbidden()
    {
        var resource = new BookableResource(Guid.NewGuid(), Guid.NewGuid(), "Room A");
        _resources.Setup(x => x.GetByIdAsync(resource.Id)).ReturnsAsync(resource);

        Assert.That(
            async () => await _sut.UpdateResource(resource.Id, "Room B"),
            Throws.TypeOf<ForbiddenException>());

        _resources.Verify(x => x.UpdateAsync(It.IsAny<BookableResource>()), Times.Never);
    }

    [Test]
    public async Task GetResources_ReturnsBookedSlotsAndPagingMetadata()
    {
        var resource = new BookableResource(_userId, "Room A");
        _resources.Setup(x => x.GetPagedAsync("room", null, 1, 10))
            .ReturnsAsync(([resource], 1));
        _bookings.Setup(x => x.GetActiveBookingsByResourceIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Single() == resource.Id),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .ReturnsAsync([
                new API.Entities.Booking(
                    resource.Id,
                    _userId,
                    DateTimeOffset.UtcNow.AddHours(1),
                    DateTimeOffset.UtcNow.AddHours(2))
            ]);

        var result = await _sut.GetResources(new API.Dto.Resource.GetResourcesFilterQuery
        {
            Search = "room",
            Page = 1,
            PageSize = 10
        });

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.TotalPages, Is.EqualTo(1));
        Assert.That(result.Items.Single().BookedSlots, Has.Count.EqualTo(1));
    }
}
