import Aside from "../../core/Aside";
import Header from "../../core/Header";

import { Outlet, useNavigate, useOutletContext } from 'react-router-dom'
import { useState } from "react";
import PostClientModal from "./components/ModalClient";
import PostDetailsModal from "./components/ModalDetails";
import PostAppointment from "./components/ModalAppointment";
import RateCalculator from "./components/RateCalculator";

export type ModalType = {
    value?: "LIST" | "DETAILS" | "INSURER" | "CALCULATOR";
    show: boolean;
    uniqueId?: number;
}

type ModalChange = {
    onModalChange: (value: ModalType) => void;
}

export function useShowModal() {
    return useOutletContext<ModalChange>();
}

const Client = () => {
    const [modalInfo, setModalInfo] = useState<ModalType>({ show: false })

    const onModalChange = (value: ModalType) => setModalInfo(value)

    return <section className="flex w-full h-full">
        <Aside />

        {(modalInfo.show && modalInfo.value === "LIST") && (<PostClientModal modalAction={onModalChange}/>)}
        {(modalInfo.show && modalInfo.value === "DETAILS") && (<PostDetailsModal modalAction={onModalChange} modalInfo={modalInfo} />)}
        {(modalInfo.show && modalInfo.value === "INSURER") && (<PostAppointment modalAction={onModalChange} modalInfo={modalInfo}/>)}
        {(modalInfo.show && modalInfo.value === "CALCULATOR") && (<RateCalculator cancel={() => onModalChange({...modalInfo, show: false})}/>)}

        <div className="w-full bg-neutral-100">
            <Header />
            <Outlet context={{ onModalChange }} />
        </div>
    </section>

}

export default Client;
