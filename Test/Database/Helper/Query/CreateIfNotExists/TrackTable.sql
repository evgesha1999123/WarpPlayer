CREATE TABLE if not exists Track (
	TrackId text primary key,
	Title text,
	Cover blob,
	Artist text,
	Album text,
	AlbumArtist text null,
	Duration integer,
	FilePath text,
	Genres text,
	Year integer,
	TrackNumber integer null,
	DiscNumber integer null,
	CreatedAt integer,
	UpdatedAt integer
);