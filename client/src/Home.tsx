import {useNavigate} from "react-router";
import {useEffect} from "react";

function Home() {

    const navigate = useNavigate();

    useEffect(() => {

        // implement cookie check

        void navigate("/Login");
        return;
    })

    return (
        <div className="flex items-center justify-center min-h-screen">
            <span className="loading loading-spinner loading-lg"></span>
        </div>
    );
}
export default Home