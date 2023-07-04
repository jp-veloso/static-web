import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ModalType } from "../..";
import Modal from "../../../../core/Modal";
import { makePrivateRequest } from "../../../../core/script";
import { PostClient } from "../../../../core/types";
import { Controller, useForm } from 'react-hook-form'
import { IMaskInput } from "react-imask";

type Props = {
    modalAction: (value: ModalType ) => void;
}

const PostClientModal = ({ modalAction }:Props) => {

    const { register, control, handleSubmit } = useForm<PostClient>()
    const navigate = useNavigate()
    const [error, setError] = useState(false)

    const postClient = (data: PostClient) => {
        const params = {
            withEnrollments: data.sync
        }

        makePrivateRequest({ url: "/clients", params, method: 'POST', data })
            .then(r => {
                let id = r.data["id"]
                modalAction({value: "LIST", show: false})
                navigate(`/clients/${id}`)
            }).catch(err => {
                setError(true)
            }) 
    }

    return (
        <form onSubmit={handleSubmit(x => postClient(x))} className="absolute z-10 w-screen h-screen bg-neutral-400 bg-opacity-40 flex justify-center items-center">
            <Modal buttonTitle="Cadastrar" title="Cadastrar cliente" cancel={() => modalAction({ value: "LIST", show: false })} >
                <div>
                    <div className="flex flex-col mb-4 form-input-field">
                        <label className="text-neutral-900 text-base font-semibold ml-0.5">CNPJ</label>
                        <Controller
                            name='cnpj'
                            control={control}
                            render={({ field }) => (<IMaskInput mask="00.000.000/0000-00" unmask={true} value={field.value} onAccept={field.onChange}></IMaskInput>)}
                        />
                    </div>
                    <div className="my-3">
                        <label className=" checkbox-control text-neutral-900">
                            <input {...register('sync')} type="checkbox" />
                            <span>Sincronizar com as seguradoras</span>
                        </label>
                    </div>
                    {error && (<div className="mb-3 text-rose-700 font-bold text-center">CNPJ já cadastrado no sistema</div>)}
                </div>
            </Modal>
        </form>
    )

}

export default PostClientModal;