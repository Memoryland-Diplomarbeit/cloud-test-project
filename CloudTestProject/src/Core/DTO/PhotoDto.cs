namespace Core.DTO;

public record PhotoDto(
    string Name,
    int PhotoAlbumId,
    byte[] Photo
);