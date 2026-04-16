import {useNavigate} from "react-router";
import { authClient } from "./baseUrl";

import { useAtom } from "jotai";
import { tokenAtom, userInfoAtom } from "./Token.tsx";
import type {LoginDto, User} from "./generated-ts-client.ts";

type AuthHook = {
    user: User | null;
    login: (request: LoginDto) => Promise<void>;
    logout: () => void;
};

export const useAuth = () => {
    const [, setJwt] = useAtom(tokenAtom);
    const [user, setUser] = useAtom(userInfoAtom);
    const navigate = useNavigate();

    const login = async (request: LoginDto) => {
        const response = await authClient.login(request);

        setJwt(response.token!);

        const me = await authClient.getUserInfo();
        setUser(me);

        navigate("/dashboard");
    };


    const logout =  () => {
        setJwt(null);
        setUser(null);
        navigate("/login");
    };

    return {
        user,
        login,
        logout,
    } as AuthHook;
};