import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { ModalType } from "../..";
import Modal from "../../../../core/Modal";
import { makePrivateRequest } from "../../../../core/script";
import { Insurer } from "../../../../core/types";

type Props = {
    modalAction: (value: ModalType) => void;
    modalInfo: ModalType
}

type SelectInsurer = {
    insurerId: string;
    insurerName: string;
    checked: boolean;
}

const PostAppointment = ({ modalAction, modalInfo }: Props) => {
    const [insurers, setInsurers] = useState<SelectInsurer[]>()

    useEffect(() => {
        makePrivateRequest({ url: '/insurers/' })
            .then(async x => {
                let items = x.data as Insurer[];
                setInsurers(items.map(item => { return { insurerId: String(item.id), insurerName: item.name, checked: false } }))
            })
    }, [])

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, checked } = e.target;
        setInsurers(insurers?.map(ins => ins.insurerId === name ? { ...ins, checked: checked } : ins))
    }

    const onSubmit = () => {
        const queryString = insurers?.filter(x => x.checked).map(x => x.insurerId).join(",")
        
        makePrivateRequest({ url: `/clients/${modalInfo.uniqueId}/enrollments/appointment`, responseType: 'blob', params: {insurers: queryString } })
            .then(response => {
                const url = window.URL.createObjectURL(new Blob([response.data]))
                const link = document.createElement("a")
                link.href = url
                link.setAttribute("download", "carta_nomeacao.pdf")
                document.body.appendChild(link)
                link.click()
                document.body.removeChild(link)
            }).finally(() => {
                modalAction({ value: "INSURER", show: false })
            })
    }

    return <form className="absolute z-10 w-screen h-screen bg-neutral-400 bg-opacity-40 flex justify-center items-center" onSubmit={(e) => { e.preventDefault(); onSubmit() }}>
        <Modal buttonTitle="Gerar carta de nomeação" title="Selecione as seguradoras" cancel={() => modalAction({ value: "INSURER", show: false })}>
            <div>
                <p className="text-neutral-600 text-xl font-medium">Aqui estão as seguradoras que poderá ser gerada a carta de nomeação.</p>
                <div className="my-2">
                    <button type={"button"} className="text-violet-600 mr-3" onClick={() => setInsurers(insurers?.map(ins => { return { ...ins, checked: true } }))}>Selecionar todas</button>
                    <button type={"button"} onClick={() => setInsurers(insurers?.map(ins => { return { ...ins, checked: false } }))}>Limpar seleção</button>
                </div>
                <div className="grid grid-cols-3 mb-2">
                    {insurers && insurers.map(x => (
                        <label key={x.insurerId} className={`checkbox-control text-sm`}>
                            <input name={x.insurerId} checked={x.checked} type={"checkbox"} onChange={handleChange}></input>
                            <span>{x.insurerName}</span>
                        </label>
                    ))}
                </div>
            </div>
        </Modal>
    </form>
}

export default PostAppointment;