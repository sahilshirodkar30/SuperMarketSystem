import React, { useState, useEffect } from "react";
import "./Categories.css"; // Adjust the CSS file path
import api from "../pages/api"; // Assuming your api instance is here

const Categories = () => {
    const [classes, setClasses] = useState<{ id: number; name: string }[]>([]);
    const [form, setForm] = useState<{ name: string }>({ name: "" });
    const [popupVisible, setPopupVisible] = useState(false);
    const [actionType, setActionType] = useState<"add" | "update" | "delete">("add");
    const [selectedClass, setSelectedClass] = useState<{ id: number; name: string } | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalPages, setTotalPages] = useState(1);

    const fetchClasses = async () => {
        try {
            const response = await api.get("Categories", {
                params: { pageNumber, pageSize },
            });
            console.log("API Response:", response.data); // Log the full response for debugging
            if (response.data && Array.isArray(response.data.data)) {
                setClasses(
                    response.data.data.map((item: any) => ({
                        id: item.categoryId,
                        name: item.name,
                    }))
                );
                setTotalPages(response.data.totalPages);
            } else {
                console.error("Unexpected response format:", response.data);
                setClasses([]); // Default to an empty array
                setError("Unexpected response format.");
            }
        } catch (err) {
            console.error(err);
            setError("Failed to fetch classes.");
        }
    };

    useEffect(() => {
        fetchClasses();
    }, [pageNumber, pageSize]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setForm((prev) => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async () => {
        setLoading(true);
        setError("");

        try {
            if (actionType === "update" && selectedClass) {
                await api.put(`Categories/${selectedClass.id}`, form);
            } else if (actionType === "add") {
                await api.post("Categories", form);
            }
            fetchClasses();
            closePopup();
        } catch (err) {
            console.error(err);
            setError("Failed to save class.");
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async () => {
        if (!selectedClass) return;
        try {
            await api.delete(`Categories/${selectedClass.id}`);
            fetchClasses();
            closePopup();
        } catch (err) {
            console.error(err);
            setError("Failed to delete class.");
        }
    };

    const openPopup = (
        type: "add" | "update" | "delete",
        classData?: { id: number; name: string }
    ) => {
        setActionType(type);
        setSelectedClass(classData || null);
        setForm({ name: classData?.name || "" });
        setPopupVisible(true);
    };

    const closePopup = () => {
        setPopupVisible(false);
        setSelectedClass(null);
        setForm({ name: "" });
    };

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            setPageNumber(newPage);
        }
    };

    return (
        <>
            <div>
                <h1>List of Categories</h1>
            </div>
            <div className="container">
                {error && <div className="alert alert-danger">{error}</div>}
                <button className="btn btn-primary" onClick={() => openPopup("add")}>
                    Add Class
                </button>
                <table className="table">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {classes.map((cls) => (
                            <tr key={cls.id}>
                                <td>{cls.name}</td>
                                <td>
                                    <button
                                        className="btn btn-warning"
                                        onClick={() => openPopup("update", cls)}
                                    >
                                        Update
                                    </button>
                                    <button
                                        className="btn btn-danger"
                                        onClick={() => openPopup("delete", cls)}
                                    >
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>

                <div className="pagination">
                    <button
                        className="btn btn-secondary"
                        onClick={() => handlePageChange(pageNumber - 1)}
                        disabled={pageNumber === 1}
                    >
                        Previous
                    </button>
                    <span>
                        Page {pageNumber} of {totalPages}
                    </span>
                    <button
                        className="btn btn-secondary"
                        onClick={() => handlePageChange(pageNumber + 1)}
                        disabled={pageNumber === totalPages}
                    >
                        Next
                    </button>
                </div>

                {popupVisible && (
                    <div className="popup">
                        <div className="popup-content">
                            {actionType === "delete" ? (
                                <>
                                    <p>
                                        Are you sure you want to delete {selectedClass?.name}?
                                    </p>
                                    <button
                                        className="btn btn-danger"
                                        onClick={handleDelete}
                                        disabled={loading}
                                    >
                                        Yes, Delete
                                    </button>
                                </>
                            ) : (
                                <form
                                    onSubmit={(e) => {
                                        e.preventDefault();
                                        handleSubmit();
                                    }}
                                >
                                    <div className="mb-3">
                                        <label>Name</label>
                                        <input
                                            type="text"
                                            name="name"
                                            value={form.name}
                                            onChange={handleInputChange}
                                            className="form-control"
                                            required
                                        />
                                    </div>
                                    <button
                                        className="btn btn-primary"
                                        type="submit"
                                        disabled={loading}
                                    >
                                        {actionType === "update" ? "Update" : "Add"}
                                    </button>
                                </form>
                            )}
                            <button className="btn btn-secondary" onClick={closePopup}>
                                Cancel
                            </button>
                        </div>
                    </div>
                )}
            </div>
        </>
    );
};

export default Categories;
