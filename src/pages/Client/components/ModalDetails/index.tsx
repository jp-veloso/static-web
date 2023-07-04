import { useState, useEffect, useCallback } from "react";
import { Controller, useFieldArray, useForm } from "react-hook-form";
import { ModalType } from "../..";
import Modal from "../../../../core/Modal";
import { EnrollmentType, Insurer } from "../../../../core/types";
import ReactSelect, { SingleValue } from "react-select"
import { makePrivateRequest } from "../../../../core/script";
import { useNavigate, useParams } from "react-router-dom";
import { IMaskInput } from "react-imask";

type Props = {
    modalAction: (value: ModalType) => void;
    modalInfo: ModalType;
}

const PostDetailsModal = ({ modalAction, modalInfo }: Props) => {
    const { id } = useParams()
    const navigate = useNavigate()
    const [insurers, setInsurers] = useState<Insurer[]>()
    const [enrollments, setEnrollments] = useState<EnrollmentType[]>()
    const { register, control, handleSubmit, setValue, reset, getValues } = useForm<EnrollmentType>();
    const { fields, append, remove } = useFieldArray({ control, name: "takers" });

    const options = ["TRADICIONAL", "RECURSAL", "JUDICIAL"]

    const handleOptionsClick = (value: string) => {
        const found = fields.find(x => x.category === value)

        if (found != null) return

        append({ id: 0, category: value, balance: 0, limit: 0, rate: 0 })
    }

    const syncEnrollment = (insurers: string = '') => {
        makePrivateRequest({ url: `/clients/${id}/enrollments/reload`, method: 'POST', params: { insurers } }).then(() => {
            modalAction({ value: "DETAILS", show: false, uniqueId: modalInfo.uniqueId })
            navigate(0)
        })
    }

    const getRequiredInfo = useCallback(() => {

        makePrivateRequest({ url: `/insurers/${modalInfo.uniqueId ? modalInfo.uniqueId : ''}` }).then(async x => {

            if(modalInfo.uniqueId){
                setInsurers([x.data])
            } else {
                setInsurers(x.data)
            }

            makePrivateRequest({ url: `/clients/${id}/enrollments` }).then(response => {
                setEnrollments(response.data.data)

                if (modalInfo.uniqueId) {
                    let ens = response.data.data as EnrollmentType[]
                    let ins = x.data as Insurer

                    setFormValue(ins.id, ens)
                }
            })
        })
    }, [])

    const cleanStringDate = (value: string) => {
        value = value.substring(0, 10)
        const values = value.split('-')
        return values[2] + '/' + values[1] + '/' + values[0]
    }

    function convertStringToDate(value: string | undefined) {
        if (value === undefined || value === '')
            return '2015-01-01T00:00:00';

        if (value.includes("-"))
            return value;

        console.log('DATA: ', value)
        let dateValues = value.split('/')
        return dateValues[2] + '-' + dateValues[1] + '-' + dateValues[0] + 'T00:00:00'
    }

    const insertOrUpdateEnrollment = (data: EnrollmentType) => {

        const takers = data.takers.map(x => {
            return {
                category: x.category,
                limit: Number(String(x.limit).replaceAll(".", "").replaceAll(",", ".")),
                rate: Number(String(x.rate).replaceAll('.', '').replaceAll(',', '.')),
                balance: Number(String(x.balance).replaceAll('.', '').replaceAll(',', '.'))
            }
        })

        const date = convertStringToDate(data.expireAt)

        const payload = {
            ...data,
            expireAt: date,
            createdAt: '2015-01-01T00:00:00',
            status: data.warn ? 'ERROR' : 'CREATED',
            takers
        }

        console.log('TO SUBMIT: ', payload)

        makePrivateRequest({
            url: `/clients/${id}/enrollments${data.createdAt == "" ? '' : '/' + data.insurerId}`,
            method: data.createdAt == "" ? 'POST' : 'PUT',
            data: payload
        }).then(() => {
            reset()
            modalAction({ value: "DETAILS", show: false, uniqueId: modalInfo.uniqueId })
            navigate(0)
        })
    }

    const handleSelectChange = (e: SingleValue<Insurer>) => {
        if (e && enrollments) { setFormValue(e.id, enrollments) }
    }

    const setFormValue = (insurerId: number, enrollments: EnrollmentType[]) => {
        const found = enrollments.find(x => x.insurer.id === insurerId)

        setValue('createdAt', found?.createdAt ?? '')
        setValue('insurerId', insurerId)
        setValue('expireAt', found?.expireAt)
        setValue('rating', found?.rating)
        setValue('warn', found?.warn)
        setValue('takers', found?.takers ?? [])
    }

    useEffect(() => {
        getRequiredInfo()
    }, [getRequiredInfo])

    return (<form className="absolute z-10 w-screen h-screen bg-neutral-400 bg-opacity-40 flex justify-center items-center" onSubmit={handleSubmit(x => insertOrUpdateEnrollment(x))}>
        <Modal buttonTitle="Atualizar registros" title="Gerenciar cadastro na seguradora" cancel={() => { reset(); modalAction({ value: "DETAILS", show: false }) }}>
            {insurers && (<div>
                <label className="font-bold text-md">Selecione a seguradora</label>
                <div className="flex items-center justify-between">
                <ReactSelect
                    options={insurers}
                    getOptionLabel={(option: Insurer) => option.name}
                    getOptionValue={(option: Insurer) => String(option.id)}
                    onChange={handleSelectChange}
                    isDisabled={insurers.length == 1 ? true : false}
                    defaultValue={insurers.length == 1 ? insurers[0] : null}
                    styles={{
                        control: (baseStyles, state) => ({
                            ...baseStyles,
                            borderRadius: '8px',
                            backgroundColor: 'white',
                            border: state.isFocused ? "1px solid #a855f7" : "1px solid #d4d4d4",
                            "&:hover": {
                                border: state.isFocused ? "1px solid #a855f7" : "1px solid #d4d4d4"
                            },
                            height: '45px',
                            width: '230px',
                            boxShadow: 'none',
                        }),
                        option: (baseStyles, state) => ({
                            ...baseStyles,
                            color: state.isSelected ? 'white' : '#333',
                            backgroundColor: state.isSelected ? '#a855f7' : 'white',
                            "&:hover": {
                                backgroundColor: '#a855f7',
                                color: '#FFF'
                            }
                        }),
                        menu: (base, state) => ({
                            ...base,
                            marginTop: '4px'
                        }),
                        container: (baseStyles, state) => ({
                            ...baseStyles,
                            marginBottom: '5px',
                            marginTop: '5px'
                        })
                    }}
                    placeholder=" " />
                    <button type="button" onClick={() => syncEnrollment(String(getValues('insurerId')))} className="rounded-xl h-fit p-2 text-neutral-900 mr-2 bg-neutral-300 hover:bg-neutral-200">Sincronizar</button>
                </div>
            </div>)}
            <div className="grid grid-cols-2 gap-1">
                <div className="flex flex-col mb-2 form-input-field">
                    <label className="text-neutral-900 text-base font-semibold ml-0.5">Rating</label>
                    <input type="text" autoComplete="off" {...register('rating')} />
                </div>

                <div className="flex flex-col mb-2 form-input-field">
                    <label className="text-neutral-900 text-base font-semibold ml-0.5">Data de expiração</label>
                    <Controller
                        name="expireAt"
                        control={control}
                        render={({ field }) => (
                            <IMaskInput
                                mask={'00/00/0000'}
                                value={cleanStringDate(field.value ?? '')}
                                unmask={false}
                                onComplete={field.onChange} />
                        )} />
                </div>

                <div className="flex flex-col mb-2 col-span-2 w-full form-input-field">
                    <label className="text-neutral-900 text-base font-semibold ml-0.5">Adicionar erro</label>
                    <input type="text" autoComplete="off" {...register('warn')} />
                </div>
            </div>

            <div className="max-h-24 overflow-y-scroll scrollbar w-full">
                <table className="table-auto w-full separate">
                    <thead>
                        <tr>
                            <th className="p-1 text-sm">Categoria</th>
                            <th className="p-1 text-sm">Limite</th>
                            <th className="p-1 text-sm">Saldo</th>
                            <th className="p-1 text-sm">Taxa</th>
                        </tr>
                    </thead>
                    <tbody>
                        {fields.map((item, index) => (
                            <tr key={item.id}>
                                <td className="p-1 text-sm">{item.category}</td>
                                <td className="p-1 formated-input">
                                    <Controller
                                        name={`takers.${index}.limit`}
                                        control={control}
                                        defaultValue={0.0}
                                        render={({ field }) => (
                                            <IMaskInput
                                                mask={Number}
                                                value={String(field.value)}
                                                scale={2}
                                                thousandsSeparator="."
                                                padFractionalZeros={false}
                                                normalizeZeros={true}
                                                radix={","}
                                                mapToRadix={["."]}
                                                onComplete={field.onChange}
                                            />
                                        )}
                                    />
                                </td>
                                <td className="p-1 formated-input">
                                    <Controller
                                        name={`takers.${index}.balance`}
                                        control={control}
                                        defaultValue={0.0}
                                        render={({ field }) => (
                                            <IMaskInput
                                                mask={Number}
                                                value={String(field.value)}
                                                scale={2}
                                                thousandsSeparator="."
                                                padFractionalZeros={false}
                                                normalizeZeros={true}
                                                radix={","}
                                                mapToRadix={["."]}
                                                onComplete={field.onChange}
                                            />
                                        )}
                                    />
                                </td>
                                <td className="p-1 formated-input">
                                    <Controller
                                        name={`takers.${index}.rate`}
                                        control={control}
                                        defaultValue={0.0}
                                        render={({ field }) => (
                                            <IMaskInput
                                                mask={Number}
                                                value={String(field.value)}
                                                scale={2}
                                                thousandsSeparator="."
                                                padFractionalZeros={false}
                                                normalizeZeros={true}
                                                radix={","}
                                                mapToRadix={["."]}
                                                onComplete={field.onChange}
                                            />
                                        )}
                                    />
                                </td>
                                <td className="p-1"><input type={'button'} onClick={() => remove(index)} value="X" className="font-black text-lg text-rose-600 cursor-pointer text-xs"></input></td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div className="flex justify-evenly text-xs font-bold my-4">
                {options.map(x => (
                    <div key={x} className="w-28 cursor-pointer" onClick={() => handleOptionsClick(x)}>
                        <div className="flex-1 h-full">
                            <div className="flex items-center justify-center flex-1 p-1 border border-gray-400 rounded-full">
                                <div className="relative">
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                                    </svg>
                                </div>
                                {x}
                            </div>
                        </div>
                    </div>
                ))}

            </div>
        </Modal>
    </form>)
}

export default PostDetailsModal;