import { Outlet } from "react-router";
import FriendSidebar from "./FriendSidebar.tsx";

function AppLayout() {
    return (
        <div style={{ display: "flex", minHeight: "100vh", background: "#0a0a0a", color: "#fff" }}>
            <FriendSidebar />
            <main style={{ flex: 1, overflow: "auto" }}>
                <Outlet />
            </main>
        </div>
    );
}

export default AppLayout;
