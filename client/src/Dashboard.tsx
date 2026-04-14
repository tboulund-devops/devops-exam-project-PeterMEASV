import { useEffect, useRef, useState } from "react";
import { useAtomValue } from "jotai";
import { finalUrl } from "./baseUrl.ts";
import type { Movie } from "./generated-ts-client.ts";
import { TOKEN_KEY, tokenAtom, tokenStorage, userInfoAtom } from "./Token.tsx";

function getUserIdFromToken(token: string | null): string | null {
    if (!token) return null;
    try {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        return payload.sub ?? null;
    } catch {
        return null;
    }
}

// ── Create Movie Modal ────────────────────────────────────────────────────────

type CreateMovieModalProps = {
    onClose: () => void;
    onCreated: (movie: Movie) => void;
};

function CreateMovieModal({ onClose, onCreated }: CreateMovieModalProps) {
    const userInfo = useAtomValue(userInfoAtom);
    const [title, setTitle] = useState("");
    const [year, setYear] = useState("");
    const [description, setDescription] = useState("");
    const [starring, setStarring] = useState("");
    const [photoFile, setPhotoFile] = useState<File | null>(null);
    const [preview, setPreview] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0] ?? null;
        setPhotoFile(file);
        setPreview(file ? URL.createObjectURL(file) : null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!title.trim() || !year.trim()) return;

        setSubmitting(true);
        setError(null);

        try {
            let userId = userInfo?.id;
            if (!userId) {
                userId = getUserIdFromToken(tokenStorage.getItem(TOKEN_KEY, null)) ?? undefined;
            }
            if (!userId) {
                setError("Session expired. Please log in again.");
                return;
            }

            const formData = new FormData();
            formData.append("title", title.trim());
            formData.append("year", year);
            if (description.trim()) formData.append("description", description.trim());
            if (starring.trim()) formData.append("starring", starring.trim());
            if (photoFile) formData.append("photo", photoFile, photoFile.name);

            const token = tokenStorage.getItem(TOKEN_KEY, null);
            const response = await fetch(
                `${finalUrl}/Movie/CreateMovie?userID=${encodeURIComponent(userId)}`,
                {
                    method: "POST",
                    headers: token ? { Authorization: `Bearer ${token}` } : {},
                    body: formData,
                }
            );
            if (!response.ok) throw new Error("Failed");
            const created: Movie = await response.json();
            onCreated(created);
        } catch {
            setError("Could not create movie. Please try again.");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div style={{
            position: "fixed", inset: 0,
            background: "rgba(0,0,0,0.6)",
            display: "flex", alignItems: "center", justifyContent: "center",
            zIndex: 100,
        }} onClick={onClose} onKeyDown={(e) => {if(e.key === 'Enter') {onClose}}} role="button">
            <div style={{
                background: "#1a1a1a", borderRadius: "10px",
                padding: "28px", width: "100%", maxWidth: "440px",
                display: "flex", flexDirection: "column", gap: "14px",
            }} onClick={(e) => e.stopPropagation()} 
                onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                         e.stopPropagation();
                             }
                        }} 
                    role="button">

                <h2 style={{ margin: 0 }}>Add movie</h2>

                <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
                    <input
                        placeholder="Title *"
                        value={title}
                        onChange={e => setTitle(e.target.value)}
                        required
                        style={inputStyle}
                    />
                    <input
                        placeholder="Year *"
                        type="number"
                        value={year}
                        onChange={e => setYear(e.target.value)}
                        required
                        style={inputStyle}
                    />
                    <input
                        placeholder="Starring"
                        value={starring}
                        onChange={e => setStarring(e.target.value)}
                        style={inputStyle}
                    />
                    <textarea
                        placeholder="Description"
                        value={description}
                        onChange={e => setDescription(e.target.value)}
                        rows={3}
                        style={{ ...inputStyle, resize: "vertical" }}
                    />

                    <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                        <label style={{ fontSize: "0.85rem", color: "#aaa" }}>Poster (optional)</label>
                        <input ref={inputRef} type="file" accept="image/*" onChange={handlePhotoChange} />
                        {preview && (
                            <img src={preview} alt="Preview" style={{ maxHeight: 160, objectFit: "contain", borderRadius: "6px" }} />
                        )}
                    </div>

                    {error && <p style={{ color: "red", margin: 0 }}>{error}</p>}

                    <div style={{ display: "flex", gap: "10px", justifyContent: "flex-end" }}>
                        <button type="button" onClick={onClose} onKeyDown={(e) => {if(e.key === 'Enter') {onClose}}} role="button" style={secondaryButtonStyle}>Cancel</button>
                        <button type="submit" disabled={submitting} style={primaryButtonStyle}>
                            {submitting ? "Saving..." : "Create"}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

function Dashboard() {
    const token = useAtomValue(tokenAtom);
    const userId = useAtomValue(userInfoAtom)?.id ?? getUserIdFromToken(token);
    const [movies, setMovies] = useState<Movie[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [modalOpen, setModalOpen] = useState(false);

    useEffect(() => {
        if (!userId) {
            setLoading(false);
            return;
        }
        const token = tokenStorage.getItem(TOKEN_KEY, null);
        fetch(`${finalUrl}/Movie/GetMoviesByUser?userId=${encodeURIComponent(userId)}`, {
            headers: {
                Accept: "application/json",
                ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
        })
            .then(r => r.json())
            .then(setMovies)
            .catch(() => setError("Could not load movies."))
            .finally(() => setLoading(false));
    }, [userId]);

    const handleCreated = (movie: Movie) => {
        setMovies(prev => [...prev, movie]);
        setModalOpen(false);
    };

    return (
        <>
            {loading && <p>Loading...</p>}
            {!loading && error && <p style={{ color: "red" }}>{error}</p>}
            {!loading && !error && movies.length === 0 && <p>No movies in your collection yet.</p>}
            {!loading && !error && movies.length > 0 && (
                <div style={{
                    display: "grid",
                    gridTemplateColumns: "repeat(4, 1fr)",
                    gap: "16px",
                    padding: "16px",
                }}>
                    {movies.map(movie => (
                        <div key={movie.id} style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                            {movie.photo
                                ? <img src={movie.photo} alt={movie.title} style={{ width: "100%", aspectRatio: "2/3", objectFit: "cover", borderRadius: "6px" }} />
                                : <div style={{ width: "100%", aspectRatio: "2/3", background: "#2a2a2a", borderRadius: "6px", display: "flex", alignItems: "center", justifyContent: "center", color: "#666" }}>No poster</div>
                            }
                            <span style={{ fontWeight: "bold", fontSize: "0.9rem" }}>{movie.title}</span>
                            <span style={{ fontSize: "0.8rem", color: "#aaa" }}>{movie.year}</span>
                        </div>
                    ))}
                </div>
            )}

            <button
                onClick={() => setModalOpen(true)} onKeyDown={(e) => {if(e.key === 'Enter') {setModalOpen(true)}}} role="button"
                style={{
                    position: "fixed", bottom: "28px", right: "28px",
                    width: "56px", height: "56px", borderRadius: "50%",
                    fontSize: "28px", lineHeight: 1,
                    background: "#e50914", color: "#fff",
                    border: "none", cursor: "pointer",
                    boxShadow: "0 4px 12px rgba(0,0,0,0.4)",
                    display: "flex", alignItems: "center", justifyContent: "center",
                }}
                aria-label="Add movie"
            >
                +
            </button>

            {modalOpen && (
                <CreateMovieModal
                    onClose={() => setModalOpen(false)}
                    onCreated={handleCreated}
                />
            )}
        </>
    );
}

// ── Styles ────────────────────────────────────────────────────────────────────

const inputStyle: React.CSSProperties = {
    padding: "8px 12px",
    borderRadius: "6px",
    border: "1px solid #333",
    background: "#111",
    color: "#fff",
    fontSize: "0.95rem",
    width: "100%",
    boxSizing: "border-box",
};

const primaryButtonStyle: React.CSSProperties = {
    padding: "8px 20px",
    borderRadius: "6px",
    background: "#e50914",
    color: "#fff",
    border: "none",
    cursor: "pointer",
    fontWeight: "bold",
};

const secondaryButtonStyle: React.CSSProperties = {
    padding: "8px 20px",
    borderRadius: "6px",
    background: "#333",
    color: "#fff",
    border: "none",
    cursor: "pointer",
};

export default Dashboard;
