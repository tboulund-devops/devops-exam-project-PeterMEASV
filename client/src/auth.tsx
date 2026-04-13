import {useNavigate} from "react-router";
import { authClient } from "./baseUrl";

import { useAtom } from "jotai";
import { tokenAtom, userInfoAtom } from "./Token.tsx";
import type {LoginDTO, User} from "./generated-ts-client.ts";

type AuthHook = {
    user: User | null;
    login: (request: LoginDTO) => Promise<void>;
    logout: () => void;
};

export const useAuth = () => {
    const [, setJwt] = useAtom(tokenAtom);
    const [user, setUser] = useAtom(userInfoAtom);
    const navigate = useNavigate();

    const login = async (request: LoginDTO) => {
        const response = await authClient.login(request);

        setJwt(response.token!);

        const me = await authClient.getUserInfo();
        setUser(me);

        navigate("/dashboard");
    };


    const logout =  () => {
        setJwt(null);
        setUser(null);
        void navigate("/login");
    };

    return {
        user,
        login,
        logout,
    } as AuthHook;
};