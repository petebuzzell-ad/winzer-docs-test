export const getImageObj = (field, value) => {
    if (!filterImages) {
        return null
    }

    return filterImages[`${field}__${value}`] || null
}
