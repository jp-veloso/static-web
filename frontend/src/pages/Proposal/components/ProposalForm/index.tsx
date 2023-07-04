import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { IMaskInput } from "react-imask";
import ReactSelect from "react-select";
import { makePrivateRequest } from "../../../../core/script";
import { ContractType, Hash, ProposalItemType } from "../../../../core/types";
import './style.css'

type ProposalFormType = {
    contract: ContractType;
    modality: string;
    insuredAmount: number;
    period: number;
    salesRate: number;
    retroactivity: number;
    category: string;
    terms: Hash
}

const modalities = [
    {
        value: "FORNECEDOR", label: "Fornecedor"
    },
    {
        value: "PRESTADOR", label: "Prestador de serviços"
    },
    {
        value: "CONSTRUTOR", label: "Construtor"
    },
    {
        value: "LICITANTE", label: "Licitante"
    }
]

const retroactivities = [
    {
        value: "60", label: "Até 60 dias"
    },
    {
        value: "90", label: "Até 90 dias"
    },
    {
        value: "120", label: "Até 120 dias"
    },
    {
        value: "150", label: "Até 150 dias"
    },
    {
        value: "180", label: "Até 180 dias"
    },
    {
        value: "200", label: "Mais de 180 dias"
    }
]

type Props = {
    setProposals: (item: ProposalItemType[]) => void
    id: number;
}

const ProposalForm = ({ setProposals, id }: Props) => {
    const { register, control, formState, handleSubmit, setValue } = useForm<ProposalFormType>()
    const [disabled, setDisabled] = useState(false)
    const [licDisabled, setLicDisabled] = useState(false)

    useEffect(() => {
        setValue('contract', 'PUBLIC_CONTRACT')
        setValue('category', 'TRADICIONAL')
        setDisabled(true)
    }, [])

    const onSubmit = (data: ProposalFormType) => {
        const payload = {
            ...data,
            insuredAmount: Number(String(data.insuredAmount).replaceAll(".", "").replaceAll(",", ".")),
            salesRate: Number(String(data.salesRate).replaceAll(".", "").replaceAll(",", ".")),
            period: Number(String(data.period).replaceAll(".", "").replaceAll(",", ".")),
            retroactivity: data.retroactivity ? Number(String(data.retroactivity).replaceAll(".", "").replaceAll(",", ".")) : 1
        }

        console.log('DATA:',payload)

        makePrivateRequest({url: `/clients/${id}/enrollments/proposals`, data: payload, method: 'POST'})
        .then(x => setProposals(x.data))
    }

    return <form onSubmit={handleSubmit(x => onSubmit(x))} className="mt-2 shadow-md rounded-xl p-3 bg-white grid gap-2 grid-cols-3 grid-flow-row">
        
        <div>
            <p className="text-neutral-500 text-sm ml-0.5">Tipo de contrato</p>
            <div className="flex m-0.5">
                <label className="checkbox-control">
                    <input type={"radio"} onClick={() => { setValue(`terms.${'deceit'}`, false); setDisabled(true) }} value={"PUBLIC_CONTRACT"} {...register('contract')}></input>
                    <span>Público</span>
                </label>
                <label className="checkbox-control mx-2">
                    <input type={"radio"} onClick={() => setDisabled(false)} value={"PRIVATE_CONTRACT"} {...register('contract')}></input>
                    <span>Privado</span>
                </label>
            </div>
        </div>
        <div>
            <label className="proposal-input">
                <span className="text-neutral-500 text-sm ml-0.5">Taxa a vender (%)</span>
                <Controller
                    name={"salesRate"}
                    rules={{ required: true }}
                    control={control}
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
            </label>
        </div>
        
        <div className="flex flex-col row-span-3">
            <p className="text-neutral-500 text-sm ml-0.5">Opções do contrato</p>
            <label className={`checkbox-control m-1 text-sm ${licDisabled ? 'text-neutral-400' : ''}`}>
                <input type={"checkbox"} {...register(`terms.${'security'}`)} disabled={licDisabled}></input>
                <span>Trabalhista e previdenciário</span>
            </label>
            <label className={`checkbox-control m-1 text-sm ${disabled ? 'text-neutral-400' : ''}`}>
                <input type={"checkbox"} {...register(`terms.${'penalties'}`)} disabled={disabled || licDisabled}></input>
                <span>Multas e penalidades</span>
            </label>
            <label className={`checkbox-control m-1 text-sm ${licDisabled ? 'text-neutral-400' : ''}`}>
                <input type={"checkbox"} {...register(`terms.${'exclusive'}`)} disabled={licDisabled}></input>
                <span>Dedicação exclusiva</span>
            </label>
            <label className="checkbox-control m-1 text-sm">
                <input type={"checkbox"} {...register(`terms.${'law'}`)}></input>
                <span>Lei 13.303/2016</span>
            </label>
            <label className="checkbox-control m-1 text-sm">
                <input type={"checkbox"} {...register(`terms.${'deceit'}`)}></input>
                <span>Culpa ou dolo</span>
            </label>
            <label className="checkbox-control m-1 text-sm">
                <input type={"checkbox"} {...register(`terms.${'corruption'}`)}></input>
                <span>Cláusula anticorrupção</span>
            </label>
            <label className="checkbox-control m-1 text-sm">
                <input type={"checkbox"} {...register(`terms.${'irrevocable'}`)}></input>
                <span>Inalienabilidade</span>
            </label>
        </div>
        

        <div>
            <label className="proposal-input">
                <span className="text-neutral-500 text-sm ml-0.5">Modalidade</span>
                <Controller
                name={"modality"}
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                    <ReactSelect
                    options={modalities}
                    getOptionLabel={(option) => option.label}
                    getOptionValue={(option) => option.value}
                    value={modalities.find(x => x.value === field.value)}
                    onChange={x => {
                        console.log('mudou')
                        if(x?.value === 'LICITANTE'){
                            setLicDisabled(true)
                        } else {
                            setLicDisabled(false)
                        }
                        field.onChange(x?.value)
                    }}
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
                    placeholder="Selecione" />
                )}
                />
            </label>
        </div>
        
        <div>
            <label className="proposal-input">
                <span className="text-neutral-500 text-sm ml-0.5">Importância Segurada (R$)</span>
                <Controller
                    name={"insuredAmount"}
                    control={control}
                    rules={{ required: true }}
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
            </label>
        </div>
        <div>
            <label className="proposal-input">
                <span className="text-neutral-500 text-sm ml-0.5">Retroatividade (dias)</span>
                <Controller
                name={"retroactivity"}
                control={control}
                render={({ field }) => (
                    <ReactSelect
                    isDisabled={licDisabled}
                    options={retroactivities}
                    getOptionLabel={(option) => option.label}
                    getOptionValue={(option) => option.value}
                    value={retroactivities.find(x => Number(x.value) === field.value)}
                    onChange={x => field.onChange(x?.value)}
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
                    placeholder="Selecione" />
                )}
                />
            </label>
        </div>
        <div>
            <label className="proposal-input">
                <span className="text-neutral-500 text-sm ml-0.5">Vigência (dias)</span>
                <Controller
                    name={"period"}
                    control={control}
                    rules={{ required: true }}
                    render={({ field }) => (
                        <IMaskInput
                            mask={Number}
                            value={String(field.value)}
                            onComplete={field.onChange}
                        />
                    )}
                />
            </label>
        </div>
        <div className="col-span-3 text-end flex items-center justify-end">
            <button type="submit" disabled={!formState.isValid} className="auth-button rounded-xl p-2 mr-6 text-white w-52 h-fit">Calcular</button>
        </div>  
    </form>
}

export default ProposalForm;